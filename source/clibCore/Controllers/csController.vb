
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        Public Function getFormInput(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal HeightLines As Integer = 1, Optional ByVal WidthRows As Integer = 40, Optional ByVal HtmlId As String = "") As String
            Return cpCore.html.html_GetFormInputCS(csPtr, ContentName, FieldName, HeightLines, WidthRows, HtmlId)
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
                result = cpCore.html.main_cs_getRecordAddLink(csPtr, PresetNameValueList, AllowPaste)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                result = cpCore.html.cs_cs_getRecordEditLink(csPtr, AllowCut)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the text value stored in the field. For Lookup fields, this method returns the name of the foreign key record.
        ''' For textFile fields, this method returns the filename.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function getText(ByVal FieldName As String) As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_get(csPtr, FieldName)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
        ''' <summary>
        ''' returns the text stored in a textfile type field instead of the filename.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function getTextFile(ByVal FieldName As String) As String
            Dim result As String = String.Empty
            Try
                result = cpCore.db.cs_getTextFile(csPtr, FieldName)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As Date)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As Boolean)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As String)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As Double)
            Call cpCore.db.cs_set(csPtr, FieldName, FieldValue)
        End Sub
        Public Sub setField(ByVal FieldName As String, ByVal FieldValue As Integer)
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
            Call cs_setFormInput(cpCore, csPtr, FieldName, RequestName)
        End Sub
        '
        '
        '
        Public Shared Sub cs_setFormInput(cpcore As coreClass, ByVal CSPointer As Integer, ByVal FieldName As String, Optional ByVal RequestName As String = "")
            Dim LocalRequestName As String
            Dim Filename As String
            Dim Path As String
            '
            'If Not (true) Then Exit Sub
            '
            If Not cpcore.db.cs_ok(CSPointer) Then
                Throw New ApplicationException("ContentSetPointer is invalid, empty, or end-of-file")
            ElseIf Trim(FieldName) = "" Then
                Throw New ApplicationException("FieldName is invalid or blank")
            Else
                LocalRequestName = RequestName
                If LocalRequestName = "" Then
                    LocalRequestName = FieldName
                End If
                Select Case cpcore.db.cs_getFieldTypeId(CSPointer, FieldName)
                    Case FieldTypeIdBoolean
                        '
                        ' Boolean
                        '
                        Call cpcore.db.cs_set(CSPointer, FieldName, cpcore.docProperties.getBoolean(LocalRequestName))
                    Case FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdManyToMany
                        '
                        ' Numbers
                        '
                        Call cpcore.db.cs_set(CSPointer, FieldName, cpcore.docProperties.getNumber(LocalRequestName))
                    Case FieldTypeIdDate
                        '
                        ' Date
                        '
                        Call cpcore.db.cs_set(CSPointer, FieldName, cpcore.docProperties.getDate(LocalRequestName))
                    Case FieldTypeIdFile, FieldTypeIdFileImage
                        '
                        '
                        '
                        Filename = cpcore.docProperties.getText(LocalRequestName)
                        If Filename <> "" Then
                            Path = cpcore.db.cs_getFilename(CSPointer, FieldName, Filename)
                            Call cpcore.db.cs_set(CSPointer, FieldName, Path)
                            Path = genericController.vbReplace(Path, "\", "/")
                            Path = genericController.vbReplace(Path, "/" & Filename, "")
                            Call cpcore.appRootFiles.saveUpload(LocalRequestName, Path, Filename)
                        End If
                    Case Else
                        '
                        ' text files
                        '
                        Call cpcore.db.cs_set(CSPointer, FieldName, cpcore.docProperties.getText(LocalRequestName))
                End Select
            End If
        End Sub
        '
        '========================================================================
        '   main_cs_get Field, translate all fields to their best text equivalent, and encode for display
        '========================================================================
        '
        Public Shared Function main_cs_getEncodedField(cpcore As coreClass, ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim result As String = ""
            Dim ContentName As String
            Dim RecordID As Integer
            '
            If cpcore.db.cs_isFieldSupported(CSPointer, "id") And cpcore.db.cs_isFieldSupported(CSPointer, "contentcontrolId") Then
                RecordID = cpcore.db.cs_getInteger(CSPointer, "id")
                ContentName = cpcore.metaData.getContentNameByID(cpcore.db.cs_getInteger(CSPointer, "contentcontrolId"))
            End If
            result = cpcore.html.html_encodeContent10(cpcore.db.cs_get(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName)), cpcore.authContext.user.id, ContentName, RecordID, 0, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.isAuthenticated, Nothing, cpcore.authContext.isEditingAnything())
            Return result
        End Function

        '
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class

End Namespace

