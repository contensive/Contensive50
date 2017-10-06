
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class exportAsciiController
        Implements IDisposable
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- objects constructed that must be disposed
        '
        'Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- private instance storage
        '
        'Private remoteCacheDisabled As Boolean
        '
        '====================================================================================================
        '
        Public Shared Function exportAscii_GetAsciiExport(cpCore As coreClass, ByVal ContentName As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As String
            Dim result As String = ""
            Try
                Dim Delimiter As String
                Dim Copy As String = String.Empty
                Dim TableName As String
                Dim CSPointer As Integer
                Dim FieldNameVariant As String
                Dim FieldName As String
                Dim UcaseFieldName As String
                Dim iContentName As String
                Dim sb As New System.Text.StringBuilder
                Dim TestFilename As String
                '
                TestFilename = "AsciiExport" & genericController.GetRandomInteger() & ".txt"
                '
                iContentName = genericController.encodeText(ContentName)
                If PageSize = 0 Then
                    PageSize = 1000
                End If
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                '
                ' ----- Check for special case iContentNames
                '
                Call cpCore.webServer.setResponseContentType("text/plain")
                Call cpCore.html.enableOutputBuffer(False)
                TableName = genericController.GetDbObjectTableName(cpCore.metaData.getContentTablename(iContentName))
                Select Case genericController.vbUCase(TableName)
                    Case "CCMEMBERS"
                        '
                        ' ----- People and member content export
                        '
                        If Not cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                            Call sb.Append("Warning: You must be a site administrator to export this information.")
                        Else
                            CSPointer = cpCore.db.csOpen(iContentName, , "ID", False, , , ,, PageSize, PageNumber)
                            '
                            ' ----- print out the field names
                            '
                            If cpCore.db.csOk(CSPointer) Then
                                Call sb.Append("""EID""")
                                Delimiter = ","
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer)
                                Do While (FieldNameVariant <> "")
                                    FieldName = genericController.encodeText(FieldNameVariant)
                                    UcaseFieldName = genericController.vbUCase(FieldName)
                                    If (UcaseFieldName <> "USERNAME") And (UcaseFieldName <> "PASSWORD") Then
                                        Call sb.Append(Delimiter & """" & FieldName & """")
                                    End If
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer)
                                    '''DoEvents
                                Loop
                                Call sb.Append(vbCrLf)
                            End If
                            '
                            ' ----- print out the values
                            '
                            Do While cpCore.db.csOk(CSPointer)
                                If Not (cpCore.db.csGetBoolean(CSPointer, "Developer")) Then
                                    Copy = cpCore.security.encodeToken((cpCore.db.csGetInteger(CSPointer, "ID")), cpCore.profileStartTime)
                                    Call sb.Append("""" & Copy & """")
                                    Delimiter = ","
                                    FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer)
                                    Do While (FieldNameVariant <> "")
                                        FieldName = genericController.encodeText(FieldNameVariant)
                                        UcaseFieldName = genericController.vbUCase(FieldName)
                                        If (UcaseFieldName <> "USERNAME") And (UcaseFieldName <> "PASSWORD") Then
                                            Copy = cpCore.db.csGet(CSPointer, FieldName)
                                            If Copy <> "" Then
                                                Copy = genericController.vbReplace(Copy, """", "'")
                                                Copy = genericController.vbReplace(Copy, vbCrLf, " ")
                                                Copy = genericController.vbReplace(Copy, vbCr, " ")
                                                Copy = genericController.vbReplace(Copy, vbLf, " ")
                                            End If
                                            Call sb.Append(Delimiter & """" & Copy & """")
                                        End If
                                        FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer)
                                        '''DoEvents
                                    Loop
                                    Call sb.Append(vbCrLf)
                                End If
                                Call cpCore.db.csGoNext(CSPointer)
                                '''DoEvents
                            Loop
                        End If
                        ' End Case
                    Case Else
                        '
                        ' ----- All other content
                        '
                        If Not cpCore.authContext.isAuthenticatedContentManager(cpCore, iContentName) Then
                            Call sb.Append("Error: You must be a content manager to export this data.")
                        Else
                            CSPointer = cpCore.db.csOpen(iContentName, , "ID", False, , , ,, PageSize, PageNumber)
                            '
                            ' ----- print out the field names
                            '
                            If cpCore.db.csOk(CSPointer) Then
                                Delimiter = ""
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer)
                                Do While (FieldNameVariant <> "")
                                    Call cpCore.appRootFiles.appendFile(TestFilename, Delimiter & """" & FieldNameVariant & """")
                                    Delimiter = ","
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer)
                                    '''DoEvents
                                Loop
                                Call cpCore.appRootFiles.appendFile(TestFilename, vbCrLf)
                            End If
                            '
                            ' ----- print out the values
                            '
                            Do While cpCore.db.csOk(CSPointer)
                                Delimiter = ""
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer)
                                Do While (FieldNameVariant <> "")
                                    Select Case cpCore.db.cs_getFieldTypeId(CSPointer, genericController.encodeText(FieldNameVariant))
                                        Case FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTML
                                            Copy = csController.main_cs_getEncodedField(cpCore, CSPointer, genericController.encodeText(FieldNameVariant))
                                        Case FieldTypeIdLookup
                                            Copy = cpCore.db.csGetLookup(CSPointer, genericController.encodeText(FieldNameVariant))
                                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                        Case Else
                                            Copy = cpCore.db.csGetText(CSPointer, genericController.encodeText(FieldNameVariant))
                                    End Select
                                    If Copy <> "" Then
                                        Copy = genericController.vbReplace(Copy, """", "'")
                                        Copy = genericController.vbReplace(Copy, vbCrLf, " ")
                                        Copy = genericController.vbReplace(Copy, vbCr, " ")
                                        Copy = genericController.vbReplace(Copy, vbLf, " ")
                                    End If
                                    Call cpCore.appRootFiles.appendFile(TestFilename, Delimiter & """" & Copy & """")
                                    Delimiter = ","
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer)
                                    '''DoEvents
                                Loop
                                Call cpCore.appRootFiles.appendFile(TestFilename, vbCrLf)
                                Call cpCore.db.csGoNext(CSPointer)
                                '''DoEvents
                            Loop
                        End If
                End Select
                result = cpCore.appRootFiles.readFile(TestFilename)
                Call cpCore.appRootFiles.deleteFile(TestFilename)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function


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
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace