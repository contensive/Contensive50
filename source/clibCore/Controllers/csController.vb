
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    Public Class csController
        Implements IDisposable
        '
        Private cpCore As Contensive.Core.coreClass
        Private csPtr As Integer
        Private OpeningMemberID As Integer
        Protected disposed As Boolean = False
        '
        ' Constructor - Initialize the Main and Csv objects
        '
        Public Sub New(ByRef cpCore As coreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' -- call .dispose for managed objects
                    If csPtr > -1 Then
                        Call cpCore.db.cs_Close(csPtr)
                    End If
                End If
                '
                ' -- Add code here to release the unmanaged resource.
            End If
            Me.disposed = True
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' insert
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function Insert(ByVal ContentName As String) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_insertRecord(ContentName, OpeningMemberID)
                success = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function OpenRecord(ByVal ContentName As String, ByVal recordId As Integer, Optional ByVal SelectFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_open(ContentName, "id=" & recordId, , ActiveOnly, , , , SelectFieldList, 1, 1)
                success = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal pageSize As Integer = 0, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_open(ContentName, SQLCriteria, SortFieldList, ActiveOnly, , , , SelectFieldList, pageSize, PageNumber)
                success = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function openGroupUsers(ByVal GroupList As List(Of String), Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_openGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
                success = ok()
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function openGroupUsers(ByVal GroupName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            Try
                Dim groupList As New List(Of String)
                groupList.Add(GroupName)
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_openGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
                success = ok()
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function openGroupListUsers(ByVal GroupCommaList As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim result As Boolean = False
            Try
                Dim groupList As New List(Of String)
                groupList.AddRange(GroupCommaList.Split(","c))
                result = openGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function openSQL(ByVal sql As String) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_openCsSql_rev("default", sql)
                success = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Function openSQL(ByVal sql As String, ByVal DataSourcename As String, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                End If
                csPtr = cpCore.db.cs_openSql(sql, DataSourcename, PageSize, PageNumber)
                success = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        Public Sub Close()
            Try
                If csPtr <> -1 Then
                    Call cpCore.db.cs_Close(csPtr)
                    csPtr = -1
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        Public Function getFormInput(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal HeightLines As Integer = 1, Optional ByVal WidthRows As Integer = 40, Optional ByVal HtmlId As String = "") As String
            Return cpCore.html_GetFormInputCS(csPtr, ContentName, FieldName, HeightLines, WidthRows, HtmlId)
        End Function
        '
        '====================================================================================================
        Public Sub delete()
            Call cpCore.db.cs_deleteRecord(csPtr)
        End Sub
        '
        '====================================================================================================
        Public Function fieldOK(ByVal FieldName As String) As Boolean
            Return cpCore.db.cs_isFieldSupported(csPtr, FieldName)
        End Function
        '
        '====================================================================================================
        Public Sub goFirst()
            Call cpCore.db.cs_goFirst(csPtr, False)
        End Sub
        '
        '====================================================================================================
        Public Function getAddLink(Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            Dim result As String = ""
            Try
                result = cpCore.main_cs_getRecordAddLink(csPtr, PresetNameValueList, AllowPaste)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function getBoolean(ByVal FieldName As String) As Boolean
            Return cpCore.db.cs_getBoolean(csPtr, FieldName)
        End Function
        '
        '====================================================================================================
        Public Function getDate(ByVal FieldName As String) As Date
            Return cpCore.db.cs_getDate(csPtr, FieldName)
        End Function
        '
        '====================================================================================================
        Public Function getEditLink(Optional ByVal AllowCut As Boolean = False) As String
            Dim result As String = String.Empty
            Try
                result = cpCore.cs_cs_getRecordEditLink(csPtr, AllowCut)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function getFilename(ByVal FieldName As String, Optional ByVal OriginalFilename As String = "", Optional ByVal ContentName As String = "") As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_getFilename(csPtr, FieldName, OriginalFilename, ContentName)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function getInteger(ByVal FieldName As String) As Integer
            Return cpCore.db.cs_getInteger(csPtr, FieldName)
        End Function
        '
        '====================================================================================================
        Public Function getNumber(ByVal FieldName As String) As Double
            Return cpCore.db.cs_getNumber(csPtr, FieldName)
        End Function
        '
        '====================================================================================================
        Public Function getRowCount() As Integer
            Return cpCore.db.cs_getRowCount(csPtr)
        End Function
        '
        '====================================================================================================
        Public Function getSql() As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_getSource(csPtr)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function getText(ByVal FieldName As String) As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_get(csPtr, FieldName)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ' Needs to be implemented (refactor to check the field type. if not fieldTypeHtml, encodeHtml)
        Public Function getHtml(ByVal FieldName As String) As String
            Return getText(FieldName)
        End Function
        '
        '====================================================================================================
        Public Function getTextFile(ByVal FieldName As String) As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_getTextFile(csPtr, FieldName)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Sub goNext()
            Call cpCore.db.cs_goNext(csPtr)
        End Sub
        '
        '====================================================================================================
        Public Function nextOK() As Boolean
            Dim result As Boolean = False
            Try
                Call cpCore.db.cs_goNext(csPtr)
                result = cpCore.db.cs_ok(csPtr)
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Function ok() As Boolean
            Return cpCore.db.cs_ok(csPtr)
        End Function
        '
        '====================================================================================================
        Public Sub save()
            Call cpCore.db.cs_save2(csPtr)
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As Object)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        '
        '====================================================================================================
        Public Sub SetField(ByVal FieldName As String, ByVal FieldValue As String)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        '
        '====================================================================================================
        Public Sub setFile(ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            Call cpCore.db.SetCSTextFile(csPtr, FieldName, Copy, ContentName)
        End Sub
        '
        '====================================================================================================
        Public Sub SetFormInput(ByVal FieldName As String, Optional ByVal RequestName As String = "")
            Call cpCore.cs_setFormInput(csPtr, FieldName, RequestName)
        End Sub
        '
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class

End Namespace

