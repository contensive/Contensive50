Option Strict On

Imports Contensive.Core.ccCommonModule
Imports System.Xml
Imports System.Runtime.InteropServices

'
Namespace Contensive.Core
    Public Class propertyCacheClass
        '
        Private cpCore As cpCoreClass
        '
        ' visit property cache
        '
        Private propertyCache(,) As String
        Private propertyCache_nameIndex As keyPtrIndexClass
        Private propertyCacheLoaded As Boolean = False
        Private propertyCacheCnt As Integer
        Private propertyTypeId As Integer
        '
        '==============================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="propertyTypeId"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass, propertyTypeId As Integer)
            Me.cpCore = cpCore
            Me.propertyTypeId = propertyTypeId
        End Sub
        '
        ' key is like vistiId, vistorId, userId
        '
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As String, ByVal keyId As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "SetCSv_VisitProperty" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim SQLNow As String
            Dim app As appServicesClass = cpCore.app
            '
            Ptr = -1
            If Not propertyCacheLoaded Then
                Call loadFromDb(keyId)
            End If
            '
            If propertyCacheCnt > 0 Then
                Ptr = propertyCache_nameIndex.getPtr(propertyName)
            End If
            If Ptr < 0 Then
                Ptr = propertyCacheCnt
                propertyCacheCnt = propertyCacheCnt + 1
                ReDim Preserve propertyCache(2, Ptr)
                propertyCache(0, Ptr) = propertyName
                propertyCache(1, Ptr) = PropertyValue
                Call propertyCache_nameIndex.setPtr(propertyName, Ptr)
                '
                ' insert a new property record, get the ID back and save it in cache
                '
                CS = app.db_csInsertRecord("Properties", SystemMemberID)
                If app.db_csOk(CS) Then
                    propertyCache(2, Ptr) = app.db_GetCSText(CS, "ID")
                    Call app.db_setCS(CS, "name", propertyName)
                    Call app.db_setCS(CS, "FieldValue", PropertyValue)
                    Call app.db_setCS(CS, "TypeID", propertyTypeId)
                    Call app.db_setCS(CS, "KeyID", CStr(keyId))
                End If
                Call app.db_csClose(CS)
            ElseIf propertyCache(1, Ptr) <> PropertyValue Then
                propertyCache(1, Ptr) = PropertyValue
                RecordID = EncodeInteger(propertyCache(2, Ptr))
                SQLNow = EncodeSQLDate(Now)
                '
                ' save the value in the property that was found
                '
                Call app.executeSql("update ccProperties set FieldValue=" & EncodeSQLText(PropertyValue) & ",ModifiedDate=" & SQLNow & " where id=" & RecordID)
            End If
            '

            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException( New Exception("Unexpected exception"))
        End Sub
        '
        '
        '
        Public Function getProperty(ByVal propertyName As String, ByVal defaultValue As String, ByVal keyId As Integer) As String
            Dim returnString As String = ""
            Try
                Dim Ptr As Integer
                Dim Found As Boolean
                '
                Ptr = -1
                If Not propertyCacheLoaded Then
                    Call loadFromDb(keyId)
                End If
                '
                If propertyCacheCnt > 0 Then
                    Ptr = propertyCache_nameIndex.getPtr(propertyName)
                    If Ptr >= 0 Then
                        returnString = EncodeText(propertyCache(1, Ptr))
                        Found = True
                    End If
                End If
                '
                If Not Found Then
                    returnString = defaultValue
                    Call setProperty(propertyName, defaultValue, keyId)
                End If
            Catch ex As Exception
                cpCore.handleException( ex)
            End Try
            Return returnString
        End Function
        '
        '
        '
        Private Sub loadFromDb(ByVal keyId As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "LoadVisitProperties" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Name As String
            Dim app As appServicesClass = cpCore.app
            '
            propertyCache_nameIndex = New keyPtrIndexClass
            propertyCacheCnt = 0
            '
            Using dt As DataTable = app.executeSql("select Name,FieldValue,ID from ccProperties where (active<>0)and(TypeID=" & propertyTypeId & ")and(KeyID=" & keyId & ")")
                If dt.Rows.Count > 0 Then
                    propertyCacheCnt = 0
                    ReDim propertyCache(2, dt.Rows.Count - 1)
                    For Each dr As DataRow In dt.Rows
                        Name = EncodeText(dr(0))
                        propertyCache(0, propertyCacheCnt) = Name
                        propertyCache(1, propertyCacheCnt) = EncodeText(dr(1))
                        propertyCache(2, propertyCacheCnt) = EncodeInteger(dr(2)).ToString
                        Call propertyCache_nameIndex.setPtr(LCase(Name), propertyCacheCnt)
                        propertyCacheCnt += 1
                    Next
                    propertyCacheCnt = dt.Rows.Count
                End If
            End Using
            '
            propertyCacheLoaded = True
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException( New Exception("Unexpected exception"))
        End Sub
    End Class
End Namespace
