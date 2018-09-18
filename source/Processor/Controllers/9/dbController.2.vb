
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports Contensive
Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Models.Context

Namespace Contensive.Core.Controllers
    Public Class dbController
        Implements IDisposable

        '
        '========================================================================
        ''' <summary>
        ''' Close a csv_ContentSet
        ''' Closes a currently open csv_ContentSet
        ''' sets CSPointer to -1
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="AsyncSave"></param>
        Public Sub csClose(ByRef CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If (CSPointer > 0) And (CSPointer <= contentSetStoreCount) Then
                    With contentSetStore(CSPointer)
                        If .IsOpen Then
                            Call csSave2(CSPointer, AsyncSave)
                            ReDim .readCache(0, 0)
                            ReDim .fieldNames(0)
                            .ResultColumnCount = 0
                            .readCacheRowCnt = 0
                            .readCacheRowPtr = -1
                            .ResultEOF = True
                            .IsOpen = False
                            If (Not .dt Is Nothing) Then
                                .dt.Dispose()
                            End If
                        End If
                    End With
                    CSPointer = -1
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the next row
        '========================================================================
        '
        Public Sub csGoNext(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                '
                Dim ContentName As String
                Dim RecordID As Integer
                '
                If Not csOk(CSPointer) Then
                    '
                    Throw New ApplicationException("CSPointer Not csv_IsCSOK.")
                Else
                    Call csSave2(CSPointer, AsyncSave)
                    'contentSetStore(CSPointer).WorkflowEditingMode = False
                    '
                    ' Move to next row
                    '
                    contentSetStore(CSPointer).readCacheRowPtr = contentSetStore(CSPointer).readCacheRowPtr + 1
                    If Not cs_IsEOF(CSPointer) Then
                        '
                        ' Not EOF
                        '
                        ' Call cs_loadCurrentRow(CSPointer)
                        '
                        ' Set Workflow Edit Mode from Request and EditLock state
                        '
                        If (Not cs_IsEOF(CSPointer)) Then
                            ContentName = contentSetStore(CSPointer).ContentName
                            RecordID = csGetInteger(CSPointer, "ID")
                            If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID) Then
                                Call cpCore.workflow.setEditLock(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the first row
        '========================================================================
        '
        Public Sub cs_goFirst(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    Call csSave2(CSPointer, AsyncSave)
                    contentSetStore(CSPointer).readCacheRowPtr = 0
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' getField returns a value from a nameValue dataset specified by the cs pointer. get the value of a field within a csv_ContentSet,  if CS in authoring mode, it gets the edit record value, except ID field. otherwise, it gets the live record value.
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getValue(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnValue As String = ""
            Try
                Dim fieldFound As Boolean
                Dim ColumnPointer As Integer
                Dim fieldNameTrimUpper As String
                Dim fieldNameTrim As String
                '
                fieldNameTrim = FieldName.Trim()
                fieldNameTrimUpper = genericController.vbUCase(fieldNameTrim)
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("Attempt To GetValue fieldname[" & FieldName & "], but the dataset Is empty Or does Not point To a valid row")
                Else
                    With contentSetStore(CSPointer)
                        '
                        '
                        fieldFound = False
                        If .writeCache.Count > 0 Then
                            '
                            ' ----- something has been set in buffer, check it first
                            '
                            If .writeCache.ContainsKey(FieldName.ToLower) Then
                                returnValue = .writeCache.Item(FieldName.ToLower)
                                fieldFound = True
                            End If
                        End If
                        If Not fieldFound Then
                            '
                            ' ----- attempt read from readCache
                            '
                            If useCSReadCacheMultiRow Then
                                If Not .dt.Columns.Contains(FieldName.ToLower) Then
                                    If (.Updateable) Then
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found in [" & .ContentName & "] with selected fields [" & .SelectTableFieldList & "]")
                                    Else
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found in sql [" & .Source & "]")
                                    End If
                                Else
                                    returnValue = genericController.encodeText(.dt.Rows(.readCacheRowPtr).Item(FieldName.ToLower))
                                End If
                            Else
                                '
                                ' ----- read the value from the Recordset Result
                                '
                                If .ResultColumnCount > 0 Then
                                    For ColumnPointer = 0 To .ResultColumnCount - 1
                                        If .fieldNames(ColumnPointer) = fieldNameTrimUpper Then
                                            returnValue = .readCache(ColumnPointer, 0)
                                            If .Updateable And (.ContentName <> "") And (FieldName <> "") Then
                                                If .CDef.fields(FieldName.ToLower()).Scramble Then
                                                    returnValue = genericController.TextDeScramble(cpCore, genericController.encodeText(returnValue))
                                                End If
                                            End If
                                            Exit For
                                        End If
                                    Next
                                    If ColumnPointer = .ResultColumnCount Then
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found In csv_ContentSet from source [" & .Source & "]")
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the first fieldname in the CS, Returns null if there are no more
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function cs_getFirstFieldName(ByVal CSPointer As Integer) As String
            Dim returnFieldName As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    contentSetStore(CSPointer).fieldPointer = 0
                    returnFieldName = cs_getNextFieldName(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldName
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the next fieldname in the CS, Returns null if there are no more
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function cs_getNextFieldName(ByVal CSPointer As Integer) As String
            Dim returnFieldName As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    With contentSetStore(CSPointer)
                        If useCSReadCacheMultiRow Then
                            Do While (returnFieldName = "") And (.fieldPointer < .ResultColumnCount)
                                returnFieldName = .fieldNames(.fieldPointer)
                                .fieldPointer = .fieldPointer + 1
                            Loop
                        Else
                            Do While (returnFieldName = "") And (.fieldPointer < .dt.Columns.Count)
                                returnFieldName = .dt.Columns(.fieldPointer).ColumnName
                                .fieldPointer = .fieldPointer + 1
                            Loop
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldName
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the type of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getFieldTypeId(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            Dim returnFieldTypeid As Integer = 0
            Try
                If csOk(CSPointer) Then
                    If contentSetStore(CSPointer).Updateable Then
                        With contentSetStore(CSPointer).CDef
                            If .Name <> "" Then
                                returnFieldTypeid = .fields(FieldName.ToLower()).fieldTypeId
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldTypeid
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the caption of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getFieldCaption(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnResult As String = ""
            Try
                If csOk(CSPointer) Then
                    If contentSetStore(CSPointer).Updateable Then
                        With contentSetStore(CSPointer).CDef
                            If .Name <> "" Then
                                returnResult = .fields(FieldName.ToLower()).caption
                                If String.IsNullOrEmpty(returnResult) Then
                                    returnResult = FieldName
                                End If
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get a list of captions of fields within a data set
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Public Function cs_getSelectFieldList(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If csOk(CSPointer) Then
                    If useCSReadCacheMultiRow Then
                        returnResult = Join(contentSetStore(CSPointer).fieldNames, ",")
                    Else
                        returnResult = contentSetStore(CSPointer).SelectTableFieldList
                        If String.IsNullOrEmpty(returnResult) Then
                            With contentSetStore(CSPointer)
                                If Not (.dt Is Nothing) Then
                                    If .dt.Columns.Count > 0 Then
                                        For FieldPointer = 0 To .dt.Columns.Count - 1
                                            returnResult = returnResult & "," & .dt.Columns(FieldPointer).ColumnName
                                        Next
                                        returnResult = Mid(returnResult, 2)
                                    End If
                                End If
                            End With
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the caption of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_isFieldSupported(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("Field name cannot be blank")
                ElseIf Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    Dim CSSelectFieldList As String = cs_getSelectFieldList(CSPointer)
                    returnResult = genericController.IsInDelimitedString(CSSelectFieldList, FieldName, ",")
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the filename that backs the field specified. only valid for fields of TextFile and File type.
        ''' Attempt to read the filename from the field
        ''' if no filename, attempt to create it from the tablename-recordid
        ''' if no recordid, create filename from a random
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="OriginalFilename"></param>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function csGetFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "", Optional fieldTypeId As Integer = 0) As String
            Dim returnFilename As String = ""
            Try
                Dim TableName As String
                Dim RecordID As Integer
                Dim fieldNameUpper As String
                Dim LenOriginalFilename As Integer
                Dim LenFilename As Integer
                Dim Pos As Integer
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("CSPointer does Not point To a valid dataset, it Is empty, Or it Is Not pointing To a valid row.")
                ElseIf FieldName = "" Then
                    Throw New ArgumentException("Fieldname Is blank")
                Else
                    fieldNameUpper = genericController.vbUCase(Trim(FieldName))
                    returnFilename = cs_getValue(CSPointer, fieldNameUpper)
                    If returnFilename <> "" Then
                        '
                        ' ----- A filename came from the record
                        '
                        If OriginalFilename <> "" Then
                            '
                            ' ----- there was an original filename, make sure it matches the one in the record
                            '
                            LenOriginalFilename = OriginalFilename.Length()
                            LenFilename = returnFilename.Length()
                            Pos = (1 + LenFilename - LenOriginalFilename)
                            If Pos <= 0 Then
                                '
                                ' Original Filename changed, create a new csv_cs_getFilename
                                '
                                returnFilename = ""
                            ElseIf Mid(returnFilename, Pos) <> OriginalFilename Then
                                '
                                ' Original Filename changed, create a new csv_cs_getFilename
                                '
                                returnFilename = ""
                            End If
                        End If
                    End If
                    If returnFilename = "" Then
                        With contentSetStore(CSPointer)
                            '
                            ' ----- no filename present, get id field
                            '
                            If .ResultColumnCount > 0 Then
                                For FieldPointer = 0 To .ResultColumnCount - 1
                                    If genericController.vbUCase(.fieldNames(FieldPointer)) = "ID" Then
                                        RecordID = csGetInteger(CSPointer, "ID")
                                        Exit For
                                    End If
                                Next
                            End If
                            '
                            ' ----- Get tablename
                            '
                            If .Updateable Then
                                '
                                ' Get tablename from Content Definition
                                '
                                ContentName = .CDef.Name
                                TableName = .CDef.ContentTableName
                            ElseIf ContentName <> "" Then
                                '
                                ' CS is SQL-based, use the contentname
                                '
                                TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                            Else
                                '
                                ' no Contentname given
                                '
                                Throw New ApplicationException("Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.")
                            End If
                            '
                            ' ----- Create filename
                            '
                            If fieldTypeId = 0 Then
                                If ContentName = "" Then
                                    If OriginalFilename = "" Then
                                        fieldTypeId = FieldTypeIdText
                                    Else
                                        fieldTypeId = FieldTypeIdFile
                                    End If
                                ElseIf (.Updateable) Then
                                    '
                                    ' -- get from cdef
                                    fieldTypeId = .CDef.fields(FieldName.ToLower()).fieldTypeId
                                Else
                                    '
                                    ' -- else assume text
                                    If OriginalFilename = "" Then
                                        fieldTypeId = FieldTypeIdText
                                    Else
                                        fieldTypeId = FieldTypeIdFile
                                    End If
                                End If
                            End If
                            If (OriginalFilename = "") Then
                                returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId)
                            Else
                                returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename)
                            End If
                            ' 20160607 - no, if you call the cs_set, it stack-overflows. this is a get, so do not save it here.
                            'Call cs_set(CSPointer, fieldNameUpper, returnFilename)
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFilename
        End Function
        '
        '   csv_cs_getText
        '
        Public Function csGetText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            csGetText = genericController.encodeText(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeInteger( csv_cs_getField )
        '
        Public Function csGetInteger(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            csGetInteger = genericController.EncodeInteger(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   encodeNumber( csv_cs_getField )
        '
        Public Function csGetNumber(ByVal CSPointer As Integer, ByVal FieldName As String) As Double
            csGetNumber = genericController.EncodeNumber(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '    genericController.EncodeDate( csv_cs_getField )
        '
        Public Function csGetDate(ByVal CSPointer As Integer, ByVal FieldName As String) As Date
            csGetDate = genericController.EncodeDate(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function csGetBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            csGetBoolean = genericController.EncodeBoolean(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function csGetLookup(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            csGetLookup = csGet(CSPointer, FieldName)
        End Function
        '
        '====================================================================================
        ' Set a csv_ContentSet Field value for a TextFile fieldtype
        '   Saves the value in a file and saves the filename in the field
        '
        '   CSPointer   The current Content Set Pointer
        '   FieldName   The name of the field to be saved
        '   Copy        Literal string to be saved in the field
        '   ContentName Contentname for the field to be saved
        '====================================================================================
        '
        Public Sub csSetTextFile(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                ElseIf String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("fieldName cannot be blank")
                ElseIf String.IsNullOrEmpty(ContentName) Then
                    Throw New ArgumentException("contentName cannot be blank")
                Else
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            Throw New ApplicationException("Attempting To update an unupdateable data set")
                        Else
                            Dim OldFilename As String = csGetText(CSPointer, FieldName)
                            Dim Filename As String = csGetFilename(CSPointer, FieldName, "", ContentName, FieldTypeIdFileText)
                            If OldFilename <> Filename Then
                                '
                                ' Filename changed, mark record changed
                                '
                                Call cpCore.cdnFiles.saveFile(Filename, Copy)
                                Call csSet(CSPointer, FieldName, Filename)
                            Else
                                Dim OldCopy As String = cpCore.cdnFiles.readFile(Filename)
                                If OldCopy <> Copy Then
                                    '
                                    ' copy changed, mark record changed
                                    '
                                    Call cpCore.cdnFiles.saveFile(Filename, Copy)
                                    Call csSet(CSPointer, FieldName, Filename)
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' ContentServer version of getDataRowColumnName
        ''' </summary>
        ''' <param name="dr"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function getDataRowColumnName(ByVal dr As DataRow, ByVal FieldName As String) As String
            Dim result As String = ""
            Try
                If String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                Else
                    result = dr.Item(FieldName).ToString
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' InsertContentRecordGetID
        ''' Inserts a record based on a content definition.
        ''' Returns the ID of the record, -1 if error
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        '''
        Public Function insertContentRecordGetID(ByVal ContentName As String, ByVal MemberID As Integer) As Integer
            Dim result As Integer = -1
            Try
                Dim CS As Integer = csInsertRecord(ContentName, MemberID)
                If Not csOk(CS) Then
                    Call csClose(CS)
                    Throw New ApplicationException("could not insert record in content [" & ContentName & "]")
                Else
                    result = csGetInteger(CS, "ID")
                End If
                Call csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Delete Content Record
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <param name="MemberID"></param>
        '
        Public Sub deleteContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID)
            Try
                If String.IsNullOrEmpty(ContentName) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ArgumentException("recordId must be positive value")
                Else
                    Dim CSPointer As Integer = cs_openContentRecord(ContentName, RecordID, MemberID, True, True)
                    If csOk(CSPointer) Then
                        Call csDeleteRecord(CSPointer)
                    End If
                    Call csClose(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' 'deleteContentRecords
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="Criteria"></param>
        ''' <param name="MemberID"></param>
        '
        Public Sub deleteContentRecords(ByVal ContentName As String, ByVal Criteria As String, Optional ByVal MemberID As Integer = 0)
            Try
                '
                Dim CSPointer As Integer
                Dim CDef As Models.Complex.cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentName cannot be blank")
                ElseIf String.IsNullOrEmpty(Criteria.Trim()) Then
                    Throw New ArgumentException("criteria cannot be blank")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If CDef Is Nothing Then
                        Throw New ArgumentException("ContentName [" & ContentName & "] was Not found")
                    ElseIf CDef.Id = 0 Then
                        Throw New ArgumentException("ContentName [" & ContentName & "] was Not found")
                    Else
                        '
                        ' -- treat all deletes one at a time to invalidate the primary cache
                        ' another option is invalidate the entire table (tablename-invalidate), but this also has performance problems
                        '
                        Dim invaldiateObjectList As New List(Of String)
                        CSPointer = csOpen(ContentName, Criteria, , False, MemberID, True, True)
                        Do While csOk(CSPointer)
                            invaldiateObjectList.Add(Controllers.cacheController.getCacheKey_Entity(CDef.ContentTableName, "id", csGetInteger(CSPointer, "id").ToString()))
                            Call csDeleteRecord(CSPointer)
                            Call csGoNext(CSPointer)
                        Loop
                        Call csClose(CSPointer)
                        Call cpCore.cache.invalidateContent(invaldiateObjectList)

                        '    ElseIf cpCore.siteProperties.allowWorkflowAuthoring And (false) Then
                        '    '
                        '    ' Supports Workflow Authoring, handle it record at a time
                        '    '
                        '    CSPointer = cs_open(ContentName, Criteria, , False, MemberID, True, True, "ID")
                        '    Do While cs_ok(CSPointer)
                        '        Call cs_deleteRecord(CSPointer)
                        '        Call cs_goNext(CSPointer)
                        '    Loop
                        '    Call cs_Close(CSPointer)
                        'Else
                        '    '
                        '    ' No Workflow Authoring, just delete records
                        '    '
                        '    Call DeleteTableRecords(CDef.ContentTableName, "(" & Criteria & ") And (" & CDef.ContentControlCriteria & ")", CDef.ContentDataSourceName)
                        '    If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                        '        Call cpCore.cache.invalidateObject(CDef.ContentTableName & "-invalidate")
                        '    End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Inserts a record in a content definition and returns a csv_ContentSet with just that record
        ''' If there was a problem, it returns -1
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        Public Function csInsertRecord(ByVal ContentName As String, Optional ByVal MemberID As Integer = -1) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim DateAddedString As String
                Dim CreateKeyString As String
                Dim Criteria As String
                Dim DataSourceName As String
                Dim FieldName As String
                Dim TableName As String
                Dim CDef As Models.Complex.cdefModel
                Dim DefaultValueText As String
                Dim LookupContentName As String
                Dim Ptr As Integer
                Dim lookups() As String
                Dim UCaseDefaultValueText As String
                Dim sqlList As New sqlFieldListClass
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("ContentName cannot be blank")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If (CDef Is Nothing) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    ElseIf (CDef.Id <= 0) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    Else
                        If MemberID = -1 Then
                            MemberID = cpCore.doc.authContext.user.id
                        End If
                        With CDef
                            '
                            ' no authoring, create default record in Live table
                            '
                            DataSourceName = .ContentDataSourceName
                            TableName = .ContentTableName
                            If .fields.Count > 0 Then
                                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In .fields
                                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                    With field
                                        FieldName = .nameLc
                                        If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                            Select Case genericController.vbUCase(FieldName)
                                                Case "CREATEKEY", "DATEADDED", "CREATEDBY", "CONTENTCONTROLID", "ID"
                                                    '
                                                    ' Block control fields
                                                    '
                                                Case Else
                                                    '
                                                    ' General case
                                                    '
                                                    Select Case .fieldTypeId
                                                        Case FieldTypeIdAutoIdIncrement
                                                            '
                                                            ' cannot insert an autoincremnt
                                                            '
                                                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                            '
                                                            ' ignore these fields, they have no associated DB field
                                                            '
                                                        Case FieldTypeIdBoolean
                                                            sqlList.add(FieldName, encodeSQLBoolean(genericController.EncodeBoolean(.defaultValue)))
                                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeNumber(.defaultValue)))
                                                        Case FieldTypeIdInteger, FieldTypeIdMemberSelect
                                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeInteger(.defaultValue)))
                                                        Case FieldTypeIdDate
                                                            sqlList.add(FieldName, encodeSQLDate(genericController.EncodeDate(.defaultValue)))
                                                        Case FieldTypeIdLookup
                                                            '
                                                            ' refactor --
                                                            ' This is a problem - the defaults should come in as the ID values, not the names
                                                            '   so a select can be added to the default configuration page
                                                            '
                                                            DefaultValueText = genericController.encodeText(.defaultValue)
                                                            If DefaultValueText = "" Then
                                                                DefaultValueText = "null"
                                                            Else
                                                                If .lookupContentID <> 0 Then
                                                                    LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID)
                                                                    If LookupContentName <> "" Then
                                                                        DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString()
                                                                    End If
                                                                ElseIf .lookupList <> "" Then
                                                                    UCaseDefaultValueText = genericController.vbUCase(DefaultValueText)
                                                                    lookups = Split(.lookupList, ",")
                                                                    For Ptr = 0 To UBound(lookups)
                                                                        If UCaseDefaultValueText = genericController.vbUCase(lookups(Ptr)) Then
                                                                            DefaultValueText = (Ptr + 1).ToString()
                                                                        End If
                                                                    Next
                                                                End If
                                                            End If
                                                            sqlList.add(FieldName, DefaultValueText)
                                                        Case Else
                                                            '
                                                            ' else text
                                                            '
                                                            sqlList.add(FieldName, encodeSQLText(.defaultValue))
                                                    End Select
                                            End Select
                                        End If
                                    End With
                                Next
                            End If
                            '
                            CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger)
                            DateAddedString = encodeSQLDate(Now)
                            '
                            Call sqlList.add("CREATEKEY", CreateKeyString) ' ArrayPointer)
                            Call sqlList.add("DATEADDED", DateAddedString) ' ArrayPointer)
                            Call sqlList.add("CONTENTCONTROLID", encodeSQLNumber(CDef.Id)) ' ArrayPointer)
                            Call sqlList.add("CREATEDBY", encodeSQLNumber(MemberID)) ' ArrayPointer)
                            '
                            Call insertTableRecord(DataSourceName, TableName, sqlList)
                            '
                            ' ----- Get the record back so we can use the ID
                            '
                            Criteria = "((createkey=" & CreateKeyString & ")And(DateAdded=" & DateAddedString & "))"
                            returnCs = csOpen(ContentName, Criteria, "ID DESC", False, MemberID, False, True)
                            ''
                            '' ----- Clear Time Stamp because a record changed
                            ''
                            'If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                            '    Call cpCore.cache.invalidateObject(ContentName)
                            'End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCs
        End Function        '
        '========================================================================
        ' Opens a Content Record
        '   If there was a problem, it returns -1 (not csv_IsCSOK)
        '   Can open either the ContentRecord or the AuthoringRecord (WorkflowAuthoringMode)
        '   Isolated in API so later we can save record in an Index buffer for fast access
        '========================================================================
        '
        Public Function cs_openContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Dim returnResult As Integer = -1
            Try
                If (RecordID <= 0) Then
                    ' no error, return -1 - Throw New ArgumentException("recordId is not valid [" & RecordID & "]")
                Else
                    returnResult = csOpen(ContentName, "(ID=" & encodeSQLNumber(RecordID) & ")", , False, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if csPointer is a valid dataset, and currently points to a valid row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function csOk(ByVal CSPointer As Integer) As Boolean
            Dim returnResult As Boolean = False
            Try
                If CSPointer < 0 Then
                    returnResult = False
                ElseIf (CSPointer >= contentSetStore.Count) Then
                    Throw New ArgumentException("dateset is not valid")
                Else
                    With contentSetStore(CSPointer)
                        returnResult = .IsOpen And (.readCacheRowPtr >= 0) And (.readCacheRowPtr < .readCacheRowCnt)
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' copy the current row of the source dataset to the destination dataset. The destination dataset must have been created with cs open or insert, and must contain all the fields in the source dataset.
        ''' </summary>
        ''' <param name="CSSource"></param>
        ''' <param name="CSDestination"></param>
        '========================================================================
        '
        Public Sub csCopyRecord(ByVal CSSource As Integer, ByVal CSDestination As Integer)
            Try
                Dim FieldName As String
                Dim DestContentName As String
                Dim DestRecordID As Integer
                Dim DestFilename As String
                Dim SourceFilename As String
                Dim DestCDef As Models.Complex.cdefModel
                '
                If Not csOk(CSSource) Then
                    Throw New ArgumentException("source dataset is not valid")
                ElseIf Not csOk(CSDestination) Then
                    Throw New ArgumentException("destination dataset is not valid")
                ElseIf (contentSetStore(CSDestination).CDef Is Nothing) Then
                    Throw New ArgumentException("copyRecord requires the destination dataset to be created from a cs Open or Insert, not a query.")
                Else
                    DestCDef = contentSetStore(CSDestination).CDef
                    DestContentName = DestCDef.Name
                    DestRecordID = csGetInteger(CSDestination, "ID")
                    FieldName = cs_getFirstFieldName(CSSource)
                    Do While (Not String.IsNullOrEmpty(FieldName))
                        Select Case genericController.vbUCase(FieldName)
                            Case "ID"
                            Case Else
                                '
                                ' ----- fields to copy
                                '
                                Dim sourceFieldTypeId As Integer = cs_getFieldTypeId(CSSource, FieldName)
                                Select Case sourceFieldTypeId
                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                        '
                                        ' ----- cdn file
                                        '
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", contentSetStore(CSDestination).CDef.Name, sourceFieldTypeId)
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId)
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call csSet(CSDestination, FieldName, DestFilename)
                                            Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                        '
                                        ' ----- private file
                                        '
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", DestContentName, sourceFieldTypeId)
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId)
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call csSet(CSDestination, FieldName, DestFilename)
                                            Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case Else
                                        '
                                        ' ----- value
                                        '
                                        Call csSet(CSDestination, FieldName, cs_getValue(CSSource, FieldName))
                                End Select
                        End Select
                        FieldName = cs_getNextFieldName(CSSource)
                    Loop
                    Call csSave2(CSDestination)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub   '
        '       
        '========================================================================
        ''' <summary>
        ''' Returns the Source for the csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Public Function csGetSource(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("the dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).Source
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Returns the value of a field, decoded into a text string result, if there is a problem, null is returned, this may be because the lookup record is inactive, so its not an error
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        '
        Public Function csGet(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim fieldValue As String = ""
            Try
                Dim FieldValueInteger As Integer
                Dim LookupContentName As String
                Dim LookupList As String
                Dim lookups() As String
                Dim FieldValueVariant As Object
                Dim CSLookup As Integer
                Dim fieldTypeId As Integer
                Dim fieldLookupId As Integer
                '
                ' ----- needs work. Go to fields table and get field definition
                '       then print accordingly
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("the dataset is not valid")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                Else
                    '
                    ' csv_ContentSet good
                    '
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            '
                            ' Not updateable -- Just return what is there as a string
                            '
                            Try
                                fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName))
                            Catch ex As Exception
                                Throw New ApplicationException("Error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] In source [" & .Source & "")
                            End Try
                        Else
                            '
                            ' Updateable -- enterprete the value
                            '
                            'ContentName = .ContentName
                            Dim field As Models.Complex.CDefFieldModel
                            If Not .CDef.fields.ContainsKey(FieldName.ToLower()) Then
                                Try
                                    fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName))
                                Catch ex As Exception
                                    Throw New ApplicationException("Error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] In content [" & .CDef.Name & "] With custom field list [" & .SelectTableFieldList & "")
                                End Try
                            Else
                                field = .CDef.fields(FieldName.ToLower)
                                fieldTypeId = field.fieldTypeId
                                If fieldTypeId = FieldTypeIdManyToMany Then
                                    '
                                    ' special case - recordset contains no data - return record id list
                                    '
                                    Dim RecordID As Integer
                                    Dim DbTable As String
                                    Dim ContentName As String
                                    Dim SQL As String
                                    Dim rs As DataTable
                                    If .CDef.fields.ContainsKey("id") Then
                                        RecordID = genericController.EncodeInteger(cs_getValue(CSPointer, "id"))
                                        With field
                                            ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID)
                                            DbTable = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                            SQL = "Select " & .ManyToManyRuleSecondaryField & " from " & DbTable & " where " & .ManyToManyRulePrimaryField & "=" & RecordID
                                            rs = executeQuery(SQL)
                                            If (genericController.isDataTableOk(rs)) Then
                                                For Each dr As DataRow In rs.Rows
                                                    fieldValue &= "," & dr.Item(0).ToString
                                                Next
                                                fieldValue = fieldValue.Substring(1)
                                            End If
                                        End With
                                    End If
                                ElseIf fieldTypeId = FieldTypeIdRedirect Then
                                    '
                                    ' special case - recordset contains no data - return blank
                                    '
                                    fieldTypeId = fieldTypeId
                                Else
                                    FieldValueVariant = cs_getValue(CSPointer, FieldName)
                                    If Not genericController.IsNull(FieldValueVariant) Then
                                        '
                                        ' Field is good
                                        '
                                        Select Case fieldTypeId
                                            Case FieldTypeIdBoolean
                                                '
                                                '
                                                '
                                                If genericController.EncodeBoolean(FieldValueVariant) Then
                                                    fieldValue = "Yes"
                                                Else
                                                    fieldValue = "No"
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdDate
                                                '
                                                '
                                                '
                                                If IsDate(FieldValueVariant) Then
                                                    '
                                                    ' formatdatetime returns 'wednesday june 5, 1990', which fails IsDate()!!
                                                    '
                                                    fieldValue = genericController.EncodeDate(FieldValueVariant).ToString()
                                                End If
                                            Case FieldTypeIdLookup
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldLookupId = field.lookupContentID
                                                    LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, fieldLookupId)
                                                    LookupList = field.lookupList
                                                    If (LookupContentName <> "") Then
                                                        '
                                                        ' -- First try Lookup Content
                                                        CSLookup = csOpen(LookupContentName, "ID=" & encodeSQLNumber(genericController.EncodeInteger(FieldValueVariant)), , , , , , "name", 1)
                                                        If csOk(CSLookup) Then
                                                            fieldValue = csGetText(CSLookup, "name")
                                                        End If
                                                        Call csClose(CSLookup)
                                                    ElseIf LookupList <> "" Then
                                                        '
                                                        ' -- Next try lookup list
                                                        FieldValueInteger = genericController.EncodeInteger(FieldValueVariant) - 1
                                                        If (FieldValueInteger >= 0) Then
                                                            lookups = Split(LookupList, ",")
                                                            If UBound(lookups) >= FieldValueInteger Then
                                                                fieldValue = lookups(FieldValueInteger)
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            Case FieldTypeIdMemberSelect
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldValue = getRecordName("people", genericController.EncodeInteger(FieldValueVariant))
                                                End If
                                            Case FieldTypeIdCurrency
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldValue = FormatCurrency(FieldValueVariant, 2, vbFalse, vbFalse, vbFalse)
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                                '
                                                '
                                                '
                                                fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant))
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                '
                                                '
                                                fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant))
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdText, FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                '
                                                '
                                                fieldValue = genericController.encodeText(FieldValueVariant)
                                            Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger
                                                '
                                                '
                                                '
                                                fieldValue = genericController.encodeText(FieldValueVariant)
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                '
                                                ' This case is covered before the select - but leave this here as safety net
                                                '
                                                'NeedsHTMLEncode = False
                                            Case Else
                                                '
                                                ' Unknown field type
                                                '
                                                Throw New ApplicationException("Can Not use field [" & FieldName & "] because the FieldType [" & fieldTypeId & "] Is invalid.")
                                        End Select
                                    End If
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return fieldValue
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Saves the value to the field, independant of field type, this routine accounts for the destination type, and saves the field as required (file, etc)
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="FieldValue"></param>
        '
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As String)
            Try
                Dim BlankTest As String
                Dim FieldNameLc As String
                Dim SetNeeded As Boolean
                Dim fileNameNoExt As String
                Dim ContentName As String
                Dim fileName As String
                Dim pathFilenameOriginal As String
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid or End-Of-file.")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldName cannnot be blank")
                Else
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            Throw New ApplicationException("Cannot update a contentset created from a sql query.")
                        Else
                            ContentName = .ContentName
                            FieldNameLc = Trim(FieldName).ToLower
                            If (FieldValue Is Nothing) Then
                                FieldValue = String.Empty
                            End If
                            With .CDef
                                If .Name <> "" Then
                                    Dim field As Models.Complex.CDefFieldModel
                                    If Not .fields.ContainsKey(FieldNameLc) Then
                                        Throw New ArgumentException("The field [" & FieldName & "] could Not be found In content [" & .Name & "]")
                                    Else
                                        field = .fields.Item(FieldNameLc)
                                        Select Case field.fieldTypeId
                                            Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect, FieldTypeIdManyToMany
                                            '
                                            ' Never set
                                            '
                                            Case FieldTypeIdFile, FieldTypeIdFileImage
                                                '
                                                ' Always set
                                                ' Saved in the field is the filename to the file
                                                ' csv_cs_get returns the filename
                                                ' csv_SetCS saves the filename
                                                '
                                                'FieldValueVariantLocal = FieldValueVariantLocal
                                                SetNeeded = True
                                            Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                                '
                                                ' Always set
                                                ' A virtual file is created to hold the content, 'tablename/FieldNameLocal/0000.ext
                                                ' the extension is different for each fieldtype
                                                ' csv_SetCS and csv_cs_get return the content, not the filename
                                                '
                                                ' Saved in the field is the filename of the virtual file
                                                ' TextFile, assume this call is only made if a change was made to the copy.
                                                ' Use the csv_SetCSTextFile to manage the modified name and date correctly.
                                                ' csv_SetCSTextFile uses this method to set the row changed, so leave this here.
                                                '
                                                fileNameNoExt = csGetText(CSPointer, FieldNameLc)
                                                'FieldValue = genericController.encodeText(FieldValueVariantLocal)
                                                If FieldValue = "" Then
                                                    If fileNameNoExt <> "" Then
                                                        Call cpCore.cdnFiles.deleteFile(fileNameNoExt)
                                                        'Call publicFiles.DeleteFile(fileNameNoExt)
                                                        fileNameNoExt = ""
                                                    End If
                                                Else
                                                    If fileNameNoExt = "" Then
                                                        fileNameNoExt = csGetFilename(CSPointer, FieldName, "", ContentName, field.fieldTypeId)
                                                    End If
                                                    Call cpCore.cdnFiles.saveFile(fileNameNoExt, FieldValue)
                                                    'Call publicFiles.SaveFile(fileNameNoExt, FieldValue)
                                                End If
                                                FieldValue = fileNameNoExt
                                                SetNeeded = True
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                ' public files - save as FieldTypeTextFile except if only white space, consider it blank
                                                '
                                                Dim PathFilename As String
                                                Dim FileExt As String
                                                Dim FilenameRev As Integer
                                                Dim path As String
                                                Dim Pos As Integer
                                                pathFilenameOriginal = csGetText(CSPointer, FieldNameLc)
                                                PathFilename = pathFilenameOriginal
                                                BlankTest = FieldValue
                                                BlankTest = genericController.vbReplace(BlankTest, " ", "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbCr, "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbLf, "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbTab, "")
                                                If BlankTest = "" Then
                                                    If PathFilename <> "" Then
                                                        Call cpCore.cdnFiles.deleteFile(PathFilename)
                                                        PathFilename = ""
                                                    End If
                                                Else
                                                    If PathFilename = "" Then
                                                        PathFilename = csGetFilename(CSPointer, FieldNameLc, "", ContentName, field.fieldTypeId)
                                                    End If
                                                    If Left(PathFilename, 1) = "/" Then
                                                        '
                                                        ' root file, do not include revision
                                                        '
                                                    Else
                                                        '
                                                        ' content file, add a revision to the filename
                                                        '
                                                        Pos = InStrRev(PathFilename, ".")
                                                        If Pos > 0 Then
                                                            FileExt = Mid(PathFilename, Pos + 1)
                                                            fileNameNoExt = Mid(PathFilename, 1, Pos - 1)
                                                            Pos = InStrRev(fileNameNoExt, "/")
                                                            If Pos > 0 Then
                                                                'path = PathFilename
                                                                fileNameNoExt = Mid(fileNameNoExt, Pos + 1)
                                                                path = Mid(PathFilename, 1, Pos)
                                                                FilenameRev = 1
                                                                If Not genericController.vbIsNumeric(fileNameNoExt) Then
                                                                    Pos = genericController.vbInstr(1, fileNameNoExt, ".r", vbTextCompare)
                                                                    If Pos > 0 Then
                                                                        FilenameRev = genericController.EncodeInteger(Mid(fileNameNoExt, Pos + 2))
                                                                        FilenameRev = FilenameRev + 1
                                                                        fileNameNoExt = Mid(fileNameNoExt, 1, Pos - 1)
                                                                    End If
                                                                End If
                                                                fileName = fileNameNoExt & ".r" & FilenameRev & "." & FileExt
                                                                'PathFilename = PathFilename & dstFilename
                                                                path = genericController.convertCdnUrlToCdnPathFilename(path)
                                                                'srcSysFile = config.physicalFilePath & genericController.vbReplace(srcPathFilename, "/", "\")
                                                                'dstSysFile = config.physicalFilePath & genericController.vbReplace(PathFilename, "/", "\")
                                                                PathFilename = path & fileName
                                                                'Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                            End If
                                                        End If
                                                    End If
                                                    If (pathFilenameOriginal <> "") And (pathFilenameOriginal <> PathFilename) Then
                                                        pathFilenameOriginal = genericController.convertCdnUrlToCdnPathFilename(pathFilenameOriginal)
                                                        Call cpCore.cdnFiles.deleteFile(pathFilenameOriginal)
                                                    End If
                                                    Call cpCore.cdnFiles.saveFile(PathFilename, FieldValue)
                                                End If
                                                FieldValue = PathFilename
                                                SetNeeded = True
                                            Case FieldTypeIdBoolean
                                                '
                                                ' Boolean - sepcial case, block on typed GetAlways set
                                                If genericController.EncodeBoolean(FieldValue) <> csGetBoolean(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdText
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                    If (FieldValue.Length > 255) Then
                                                        cpCore.handleException(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for Text field is 255. Save will be attempted"))
                                                    End If
                                                End If
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                    If (FieldValue.Length > 65535) Then
                                                        cpCore.handleException(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for LongText and Html is 65535. Save will be attempted"))
                                                    End If
                                                End If
                                            Case Else
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                        End Select
                                    End If
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                '
                                ' ----- set the new value into the row buffer
                                '
                                If .writeCache.ContainsKey(FieldNameLc) Then
                                    .writeCache.Item(FieldNameLc) = FieldValue.ToString()
                                Else
                                    .writeCache.Add(FieldNameLc, FieldValue.ToString())
                                End If
                                .LastUsed = DateTime.Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Date)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Boolean)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Integer)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Double)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' rollback, or undo the changes to the current row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        Public Sub csRollBack(ByVal CSPointer As Integer)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    contentSetStore(CSPointer).writeCache.Clear()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Save the current CS Cache back to the database
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="AsyncSave"></param>
        ''' <param name="Blockcsv_ClearBake"></param>
        '   If in Workflow Edit, save authorable fields to EditRecord, non-authorable to (both EditRecord and LiveRecord)
        '   non-authorable fields are inactive, non-authorable, read-only, and not-editable
        '
        ' Comment moved from in-line -- it was too hard to read around
        ' No -- IsModified is now set from an authoring control.
        '   Update all non-authorable fields in the edit record so they can be read in admin.
        '   Update all non-authorable fields in live record, because non-authorable is not a publish-able field
        '   edit record ModifiedDate in record only if non-authorable field is changed
        '
        ' ???
        '   I believe Non-FieldAdminAuthorable Fields should only save to the LiveRecord.
        '   They should also be read from the LiveRecord.
        '   Saving to the EditRecord sets the record Modified, which fields like "Viewings" should not change
        '
        '========================================================================
        '
        Public Sub csSave2(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            Try
                Dim sqlModifiedDate As Date
                Dim sqlModifiedBy As Integer
                Dim writeCacheValue As Object
                Dim UcaseFieldName As String
                Dim FieldName As String
                Dim FieldFoundCount As Integer
                Dim FieldAdminAuthorable As Boolean
                Dim FieldReadOnly As Boolean
                Dim SQL As String
                Dim SQLSetPair As String
                Dim SQLUpdate As String
                'Dim SQLEditUpdate As String
                'Dim SQLEditDelimiter As String
                Dim SQLLiveUpdate As String
                Dim SQLLiveDelimiter As String
                Dim SQLCriteriaUnique As String = String.Empty
                Dim UniqueViolationFieldList As String = String.Empty

                Dim LiveTableName As String
                Dim LiveDataSourceName As String
                Dim LiveRecordID As Integer
                'Dim EditRecordID As Integer
                Dim LiveRecordContentControlID As Integer
                Dim LiveRecordContentName As String
                'Dim EditTableName As String
                'Dim EditDataSourceName As String = ""
                Dim AuthorableFieldUpdate As Boolean            ' true if an Edit field is being updated
                'Dim WorkflowRenderingMode As Boolean
                ' Dim AllowWorkflowSave As Boolean
                Dim Copy As String
                Dim ContentID As Integer
                Dim ContentName As String
                ' Dim WorkflowMode As Boolean
                Dim LiveRecordInactive As Boolean
                Dim ColumnPtr As Integer

                '
                If Not csOk(CSPointer) Then
                    '
                    ' already closed or not opened or not on a current row. No error so you can always call save(), it skips if nothing to save
                    '
                    'Throw New ArgumentException("dataset is not valid")
                ElseIf (contentSetStore(CSPointer).writeCache.Count = 0) Then
                    '
                    ' nothing to write, just exit
                    '
                ElseIf (Not contentSetStore(CSPointer).Updateable) Then
                    Throw New ArgumentException("The dataset cannot be updated because it was created with a query and not a content table.")
                Else
                    With contentSetStore(CSPointer)
                        '
                        With .CDef
                            LiveTableName = .ContentTableName
                            LiveDataSourceName = .ContentDataSourceName
                            ContentName = .Name
                            ContentID = .Id
                        End With
                        '
                        LiveRecordID = csGetInteger(CSPointer, "ID")
                        LiveRecordContentControlID = csGetInteger(CSPointer, "CONTENTCONTROLID")
                        LiveRecordContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, LiveRecordContentControlID)
                        LiveRecordInactive = Not csGetBoolean(CSPointer, "ACTIVE")
                        '
                        '
                        SQLLiveDelimiter = ""
                        SQLLiveUpdate = ""
                        SQLLiveDelimiter = ""
                        'SQLEditUpdate = ""
                        'SQLEditDelimiter = ""
                        sqlModifiedDate = DateTime.Now
                        sqlModifiedBy = .OwnerMemberID
                        '
                        AuthorableFieldUpdate = False
                        FieldFoundCount = 0
                        For Each keyValuePair In .writeCache
                            FieldName = keyValuePair.Key
                            UcaseFieldName = genericController.vbUCase(FieldName)
                            writeCacheValue = keyValuePair.Value
                            '
                            ' field has changed
                            '
                            If UcaseFieldName = "MODIFIEDBY" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedBy = genericController.EncodeInteger(writeCacheValue)
                            ElseIf UcaseFieldName = "MODIFIEDDATE" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedDate = genericController.EncodeDate(writeCacheValue)
                            Else
                                '
                                ' let these field be added to the sql
                                '
                                LiveRecordInactive = (UcaseFieldName = "ACTIVE" And (Not genericController.EncodeBoolean(writeCacheValue)))
                                FieldFoundCount += 1
                                Dim field As Models.Complex.CDefFieldModel = .CDef.fields(FieldName.ToLower())
                                With field
                                    SQLSetPair = ""
                                    FieldReadOnly = (.ReadOnly)
                                    FieldAdminAuthorable = ((Not .ReadOnly) And (Not .NotEditable) And (.authorable))
                                    '
                                    ' ----- Set SQLSetPair to the name=value pair for the SQL statement
                                    '
                                    Select Case .fieldTypeId
                                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                        Case FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdAutoIdIncrement, FieldTypeIdMemberSelect
                                            SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeInteger(writeCacheValue))
                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                            SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeNumber(writeCacheValue))
                                        Case FieldTypeIdBoolean
                                            SQLSetPair = FieldName & "=" & encodeSQLBoolean(genericController.EncodeBoolean(writeCacheValue))
                                        Case FieldTypeIdDate
                                            SQLSetPair = FieldName & "=" & encodeSQLDate(genericController.EncodeDate(writeCacheValue))
                                        Case FieldTypeIdText
                                            Copy = Left(genericController.encodeText(writeCacheValue), 255)
                                            If .Scramble Then
                                                Copy = genericController.TextScramble(cpCore, Copy)
                                            End If
                                            SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                        Case FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTML
                                            Copy = Left(genericController.encodeText(writeCacheValue), 255)
                                            SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                        Case FieldTypeIdLongText, FieldTypeIdHTML
                                            SQLSetPair = FieldName & "=" & encodeSQLText(genericController.encodeText(writeCacheValue))
                                        Case Else
                                            '
                                            ' Invalid fieldtype
                                            '
                                            Throw New ApplicationException("Can Not save this record because the field [" & .nameLc & "] has an invalid field type Id [" & .fieldTypeId & "]")
                                    End Select
                                    If SQLSetPair <> "" Then
                                        '
                                        ' ----- Set the new value in the 
                                        '
                                        With contentSetStore(CSPointer)
                                            If .ResultColumnCount > 0 Then
                                                For ColumnPtr = 0 To .ResultColumnCount - 1
                                                    If .fieldNames(ColumnPtr) = UcaseFieldName Then
                                                        .readCache(ColumnPtr, .readCacheRowPtr) = writeCacheValue.ToString()
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End With
                                        If .UniqueName And (genericController.encodeText(writeCacheValue) <> "") Then
                                            '
                                            ' ----- set up for unique name check
                                            '
                                            If (Not String.IsNullOrEmpty(SQLCriteriaUnique)) Then
                                                SQLCriteriaUnique &= "Or"
                                                UniqueViolationFieldList &= ","
                                            End If
                                            Dim writeCacheValueText As String = genericController.encodeText(writeCacheValue)
                                            If Len(writeCacheValueText) < 255 Then
                                                UniqueViolationFieldList &= .nameLc & "=""" & writeCacheValueText & """"
                                            Else
                                                UniqueViolationFieldList &= .nameLc & "=""" & Left(writeCacheValueText, 255) & "..."""
                                            End If
                                            Select Case .fieldTypeId
                                                Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                Case Else
                                                    SQLCriteriaUnique &= "(" & .nameLc & "=" & EncodeSQL(writeCacheValue, .fieldTypeId) & ")"
                                            End Select
                                        End If
                                        '
                                        ' ----- Live mode: update live record
                                        '
                                        SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                        SQLLiveDelimiter = ","
                                        If FieldAdminAuthorable Then
                                            AuthorableFieldUpdate = True
                                        End If
                                    End If
                                End With
                            End If
                        Next
                        '
                        ' ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                        '
                        If AuthorableFieldUpdate Then
                            If (SQLLiveUpdate <> "") Then
                                '
                                ' ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                '
                                SQLLiveUpdate = SQLLiveUpdate & ",MODIFIEDDATE=" & encodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & encodeSQLNumber(sqlModifiedBy)
                            End If
                        End If
                        ''
                        '' not sure why, but this section was commented out.
                        '' Modified was not being set, so I un-commented it
                        ''
                        'If (SQLEditUpdate <> "") And (AuthorableFieldUpdate) Then
                        '    '
                        '    ' ----- set the csv_ContentSet Modified
                        '    '
                        '    Call cpCore.workflow.setRecordLocking(ContentName, LiveRecordID, AuthoringControlsModified, .OwnerMemberID)
                        'End If
                        '
                        ' ----- Do the unique check on the content table, if necessary
                        '
                        If SQLCriteriaUnique <> "" Then
                            Dim sqlUnique As String = "SELECT ID FROM " & LiveTableName & " WHERE (ID<>" & LiveRecordID & ")AND(" & SQLCriteriaUnique & ")and(" & .CDef.ContentControlCriteria & ");"
                            Using dt As DataTable = executeQuery(sqlUnique, LiveDataSourceName)
                                '
                                ' -- unique violation
                                If (dt.Rows.Count > 0) Then
                                    Throw New ApplicationException(("Can not save record to content [" & LiveRecordContentName & "] because it would create a non-unique record for one or more of the following field(s) [" & UniqueViolationFieldList & "]"))
                                End If
                            End Using
                        End If
                        If (FieldFoundCount > 0) Then
                            '
                            ' ----- update live table (non-workflowauthoring and non-authorable fields)
                            '
                            If (SQLLiveUpdate <> "") Then
                                SQLUpdate = "UPDATE " & LiveTableName & " SET " & SQLLiveUpdate & " WHERE ID=" & LiveRecordID & ";"
                                Call executeQuery(SQLUpdate, LiveDataSourceName)
                            End If
                            '
                            ' ----- Live record has changed
                            '
                            If AuthorableFieldUpdate Then
                                '
                                ' ----- reset the ContentTimeStamp to csv_ClearBake
                                '
                                Call cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(LiveTableName, "id", LiveRecordID.ToString()))
                                '
                                ' ----- mark the record NOT UpToDate for SpiderDocs
                                '
                                If (LCase(LiveTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                    If isSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                        SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                        Call executeQuery(SQL)
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=====================================================================================================
        ''' <summary>
        ''' Initialize the csv_ContentSet Result Cache when it is first opened
        ''' </summary>
        ''' <param name="CSPointer"></param>
        '
        Private Sub cs_initData(ByVal CSPointer As Integer)
            Try
                Dim ColumnPtr As Integer
                '
                With contentSetStore(CSPointer)
                    .ResultColumnCount = 0
                    .readCacheRowCnt = 0
                    .readCacheRowPtr = -1
                    .writeCache = New Dictionary(Of String, String)
                    .ResultEOF = True
                    If .dt.Rows.Count > 0 Then
                        .ResultColumnCount = .dt.Columns.Count
                        ColumnPtr = 0
                        ReDim .fieldNames(.ResultColumnCount)
                        For Each dc As DataColumn In .dt.Columns
                            .fieldNames(ColumnPtr) = genericController.vbUCase(dc.ColumnName)
                            ColumnPtr = ColumnPtr + 1
                        Next
                        ' refactor -- convert interal storage to dt and assign -- will speedup open
                        .readCache = convertDataTabletoArray(.dt)
                        .readCacheRowCnt = UBound(.readCache, 2) + 1
                        .readCacheRowPtr = 0
                    End If
                    .writeCache = New Dictionary(Of String, String)
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=====================================================================================================
        ''' <summary>
        ''' returns tru if the dataset is pointing past the last row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        '
        Private Function cs_IsEOF(ByVal CSPointer As Integer) As Boolean
            Dim returnResult As Boolean = True
            Try
                If CSPointer <= 0 Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    With contentSetStore(CSPointer)
                        cs_IsEOF = (.readCacheRowPtr >= .readCacheRowCnt)
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Encode a value for a sql
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="fieldType"></param>
        ''' <returns></returns>
        Public Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
            Dim returnResult As String = ""
            Try
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        returnResult = encodeSQLBoolean(genericController.EncodeBoolean(expression))
                    Case FieldTypeIdCurrency, FieldTypeIdFloat
                        returnResult = encodeSQLNumber(genericController.EncodeNumber(expression))
                    Case FieldTypeIdAutoIdIncrement, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
                        returnResult = encodeSQLNumber(genericController.EncodeInteger(expression))
                    Case FieldTypeIdDate
                        returnResult = encodeSQLDate(genericController.EncodeDate(expression))
                    Case FieldTypeIdLongText, FieldTypeIdHTML
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileText, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTML
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                    Case Else
                        cpCore.handleException(New ApplicationException("Unknown Field Type [" & fieldType & ""))
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' return a sql compatible string. 
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        Public Function encodeSQLText(ByVal expression As String) As String
            Dim returnResult As String = ""
            If expression Is Nothing Then
                returnResult = "null"
            Else
                returnResult = genericController.encodeText(expression)
                If returnResult = "" Then
                    returnResult = "null"
                Else
                    returnResult = "'" & genericController.vbReplace(returnResult, "'", "''") & "'"
                End If
            End If
            Return returnResult
        End Function
        Public Function encodeSqlTextLike(cpcore As coreClass, source As String) As String
            Return encodeSQLText("%" & source & "%")
        End Function
        '
        '========================================================================
        ''' <summary>
        '''    encodeSQLDate
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLDate(ByVal expression As Date) As String
            Dim returnResult As String = ""
            Try
                If IsDBNull(expression) Then
                    returnResult = "null"
                Else
                    Dim expressionDate As Date = genericController.EncodeDate(expression)
                    If (expressionDate = Date.MinValue) Then
                        returnResult = "null"
                    Else
                        returnResult = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' encodeSQLNumber
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLNumber(ByVal expression As Double) As String
            Return expression.ToString
            'Dim returnResult As String = ""
            'Try
            '    If False Then
            '        'If expression Is Nothing Then
            '        'returnResult = "null"
            '        'ElseIf VarType(expression) = vbBoolean Then
            '        '    If genericController.EncodeBoolean(expression) Then
            '        '        returnResult = SQLTrue
            '        '    Else
            '        '        returnResult = SQLFalse
            '        '    End If
            '    ElseIf Not genericController.vbIsNumeric(expression) Then
            '        returnResult = "null"
            '    Else
            '        returnResult = expression.ToString
            '    End If
            'Catch ex As Exception
            '    cpCore.handleExceptionAndContinue(ex) : Throw
            'End Try
            'Return returnResult
        End Function
        '
        Public Function encodeSQLNumber(ByVal expression As Integer) As String
            Return expression.ToString
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' encodeSQLBoolean
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLBoolean(ByVal expression As Boolean) As String
            Dim returnResult As String = SQLFalse
            Try
                If expression Then
                    returnResult = SQLTrue
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Create a filename for the Virtual Directory
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="RecordID"></param>
        ''' <param name="OriginalFilename"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function GetVirtualFilename(ByVal ContentName As String, ByVal FieldName As String, ByVal RecordID As Integer, Optional ByVal OriginalFilename As String = "") As String
            Dim returnResult As String = ""
            Try
                Dim fieldTypeId As Integer
                Dim TableName As String
                'Dim iOriginalFilename As String
                Dim CDef As Models.Complex.cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ArgumentException("recordid is not valid")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If CDef.Id = 0 Then
                        Throw New ApplicationException("contentname [" & ContentName & "] is not a valid content")
                    Else
                        TableName = CDef.ContentTableName
                        If TableName = "" Then
                            TableName = ContentName
                        End If
                        '
                        'iOriginalFilename = genericController.encodeEmptyText(OriginalFilename, "")
                        '
                        fieldTypeId = CDef.fields(FieldName.ToLower()).fieldTypeId
                        '
                        If OriginalFilename = "" Then
                            returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId)
                        Else
                            returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Opens a csv_ContentSet with the Members of a group
        ''' </summary>
        ''' <param name="groupList"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function csOpenGroupUsers(ByVal groupList As List(Of String), Optional ByVal sqlCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnResult As Integer = -1
            Try
                Dim rightNow As Date = DateTime.Now
                Dim sqlRightNow As String = encodeSQLDate(rightNow)
                '
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = pageSizeDefault
                End If
                If groupList.Count > 0 Then
                    '
                    ' Build Inner Query to select distinct id needed
                    '
                    Dim SQL As String = "SELECT DISTINCT ccMembers.id" _
                        & " FROM (ccMembers" _
                        & " LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)" _
                        & " LEFT JOIN ccGroups ON ccMemberRules.GroupID = ccGroups.ID" _
                        & " WHERE (ccMemberRules.Active<>0)AND(ccGroups.Active<>0)"
                    '
                    If ActiveOnly Then
                        SQL &= "AND(ccMembers.Active<>0)"
                    End If
                    '
                    Dim subQuery As String = ""
                    For Each groupName As String In groupList
                        If Not String.IsNullOrEmpty(groupName.Trim) Then
                            subQuery &= "or(ccGroups.Name=" & encodeSQLText(groupName.Trim) & ")"
                        End If
                    Next
                    If Not String.IsNullOrEmpty(subQuery) Then
                        SQL &= "and(" & subQuery.Substring(2) & ")"
                    End If
                    '
                    ' -- group expiration
                    SQL &= "and((ccMemberRules.DateExpires Is Null)or(ccMemberRules.DateExpires>" & sqlRightNow & "))"
                    '
                    ' Build outer query to get all ccmember fields
                    ' Must do this inner/outer because if the table has a text field, it can not be in the distinct
                    '
                    SQL = "SELECT * from ccMembers where id in (" & SQL & ")"
                    If sqlCriteria <> "" Then
                        SQL &= "and(" & sqlCriteria & ")"
                    End If
                    If SortFieldList <> "" Then
                        SQL &= " Order by " & SortFieldList
                    End If
                    returnResult = csOpenSql_rev("default", SQL, PageSize, PageNumber)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function '
        '========================================================================
        ''' <summary>
        ''' Get a Contents Tableid from the ContentPointer
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function GetContentTableID(ByVal ContentName As String) As Integer
            Dim returnResult As Integer
            Try
                Dim dt As DataTable = executeQuery("select ContentTableID from ccContent where name=" & encodeSQLText(ContentName))
                If Not genericController.isDataTableOk(dt) Then
                    Throw New ApplicationException("Content [" & ContentName & "] was not found in ccContent table")
                Else
                    returnResult = genericController.EncodeInteger(dt.Rows(0).Item("ContentTableID"))
                End If
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' csv_DeleteTableRecord
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="RecordID"></param>
        '
        Public Sub deleteTableRecord(ByVal TableName As String, ByVal RecordID As Integer, ByVal DataSourceName As String)
            Try
                If String.IsNullOrEmpty(TableName.Trim()) Then
                    Throw New ApplicationException("tablename cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ApplicationException("record id is not valid [" & RecordID & "]")
                Else
                    Call DeleteTableRecords(TableName, "ID=" & RecordID, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub     '


		End Class
End Namespace

