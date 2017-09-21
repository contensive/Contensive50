
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Context
    Public Class propertyModelClass
        '
        Private cpCore As coreClass
        '
        ' visit property cache
        '
        Private propertyCache(,) As String
        Private propertyCache_nameIndex As keyPtrController
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
        Public Sub New(cpCore As coreClass, propertyTypeId As Integer)
            Me.cpCore = cpCore
            Me.propertyTypeId = propertyTypeId
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As Double)
            setProperty(propertyName, PropertyValue.ToString())
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As Boolean)
            setProperty(propertyName, PropertyValue.ToString())
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As Date)
            setProperty(propertyName, PropertyValue.ToString())
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As Integer)
            setProperty(propertyName, PropertyValue.ToString())
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As String)
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    setProperty(propertyName, PropertyValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    setProperty(propertyName, PropertyValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    setProperty(propertyName, PropertyValue, cpCore.authContext.user.id)
            End Select
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="PropertyValue"></param>
        ''' <param name="keyId">keyId is like vistiId, vistorId, userId</param>
        Public Sub setProperty(ByVal propertyName As String, ByVal PropertyValue As String, ByVal keyId As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "SetCSv_VisitProperty" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim SQLNow As String
            Dim db As dbController = cpCore.db
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
                CS = db.cs_insertRecord("Properties", SystemMemberID)
                If db.cs_ok(CS) Then
                    propertyCache(2, Ptr) = db.cs_getText(CS, "ID")
                    Call db.cs_set(CS, "name", propertyName)
                    Call db.cs_set(CS, "FieldValue", PropertyValue)
                    Call db.cs_set(CS, "TypeID", propertyTypeId)
                    Call db.cs_set(CS, "KeyID", CStr(keyId))
                End If
                Call db.cs_Close(CS)
            ElseIf propertyCache(1, Ptr) <> PropertyValue Then
                propertyCache(1, Ptr) = PropertyValue
                RecordID = genericController.EncodeInteger(propertyCache(2, Ptr))
                SQLNow = db.encodeSQLDate(Now)
                '
                ' save the value in the property that was found
                '
                Call db.executeQuery("update ccProperties set FieldValue=" & db.encodeSQLText(PropertyValue) & ",ModifiedDate=" & SQLNow & " where id=" & RecordID)
            End If
            '

            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getDate(ByVal propertyName As String) As Date
            Return getDate(propertyName, Date.MinValue)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getDate(ByVal propertyName As String, ByVal defaultValue As Date) As Date
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    Return getDate(propertyName, defaultValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    Return getDate(propertyName, defaultValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    Return getDate(propertyName, defaultValue, cpCore.authContext.user.id)
            End Select
            Return Date.MinValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getDate(ByVal propertyName As String, ByVal defaultValue As Date, ByVal keyId As Integer) As Date
            Return genericController.EncodeDate(getText(propertyName, genericController.encodeText(defaultValue), keyId))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getNumber(ByVal propertyName As String) As Double
            Return getNumber(propertyName, 0)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getNumber(ByVal propertyName As String, ByVal defaultValue As Double) As Double
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    Return getNumber(propertyName, defaultValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    Return getNumber(propertyName, defaultValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    Return getNumber(propertyName, defaultValue, cpCore.authContext.user.id)
            End Select
            Return 0
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a boolean property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getNumber(ByVal propertyName As String, ByVal defaultValue As Double, ByVal keyId As Integer) As Double
            Return EncodeNumber(getText(propertyName, genericController.encodeText(defaultValue), keyId))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getBoolean(ByVal propertyName As String) As Boolean
            Return getBoolean(propertyName, False)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getBoolean(ByVal propertyName As String, ByVal defaultValue As Boolean) As Boolean
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    Return getBoolean(propertyName, defaultValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    Return getBoolean(propertyName, defaultValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    Return getBoolean(propertyName, defaultValue, cpCore.authContext.user.id)
            End Select
            Return False
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a boolean property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getBoolean(ByVal propertyName As String, ByVal defaultValue As Boolean, ByVal keyId As Integer) As Boolean
            Return genericController.EncodeBoolean(getText(propertyName, genericController.encodeText(defaultValue), keyId))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get an integer property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getInteger(ByVal propertyName As String) As Integer
            Return getInteger(propertyName, 0)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get an integer property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getInteger(ByVal propertyName As String, ByVal defaultValue As Integer) As Integer
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    Return getInteger(propertyName, defaultValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    Return getInteger(propertyName, defaultValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    Return getInteger(propertyName, defaultValue, cpCore.authContext.user.id)
            End Select
            Return 0
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get an integer property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getInteger(ByVal propertyName As String, ByVal defaultValue As Integer, ByVal keyId As Integer) As Integer
            Return genericController.EncodeInteger(getText(propertyName, genericController.encodeText(defaultValue), keyId))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a string property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getText(ByVal propertyName As String) As String
            Return getText(propertyName, "")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a string property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getText(ByVal propertyName As String, ByVal defaultValue As String) As String
            Select Case propertyTypeId
                Case PropertyTypeVisit
                    Return getText(propertyName, defaultValue, cpCore.authContext.visit.id)
                Case PropertyTypeVisitor
                    Return getText(propertyName, defaultValue, cpCore.authContext.visitor.ID)
                Case PropertyTypeMember
                    Return getText(propertyName, defaultValue, cpCore.authContext.user.id)
            End Select
            Return ""
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a string property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="defaultValue"></param>
        ''' <param name="keyId"></param>
        ''' <returns></returns>
        Public Function getText(ByVal propertyName As String, ByVal defaultValue As String, ByVal keyId As Integer) As String
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
                        returnString = genericController.encodeText(propertyCache(1, Ptr))
                        Found = True
                    End If
                End If
                '
                If Not Found Then
                    returnString = defaultValue
                    Call setProperty(propertyName, defaultValue, keyId)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
            Dim db As dbController = cpCore.db
            '
            propertyCache_nameIndex = New keyPtrController
            propertyCacheCnt = 0
            '
            Using dt As DataTable = db.executeQuery("select Name,FieldValue,ID from ccProperties where (active<>0)and(TypeID=" & propertyTypeId & ")and(KeyID=" & keyId & ")")
                If dt.Rows.Count > 0 Then
                    propertyCacheCnt = 0
                    ReDim propertyCache(2, dt.Rows.Count - 1)
                    For Each dr As DataRow In dt.Rows
                        Name = genericController.encodeText(dr(0))
                        propertyCache(0, propertyCacheCnt) = Name
                        propertyCache(1, propertyCacheCnt) = genericController.encodeText(dr(1))
                        propertyCache(2, propertyCacheCnt) = genericController.EncodeInteger(dr(2)).ToString
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
            cpCore.handleException(New Exception("Unexpected exception"))
        End Sub
    End Class
End Namespace
