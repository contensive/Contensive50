
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports System.Reflection
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' entity model pattern
    '   factory pattern load because if a record is not found, must rturn nothing
    '   new() - empty constructor to allow deserialization
    '   saveObject() - saves instance properties (nonstatic method)
    '   create() - loads instance properties and returns a model 
    '   delete() - deletes the record that matches the argument
    '   getObjectList() - a pattern for creating model lists.
    '   invalidateFIELDNAMEcache() - method to invalide the model cache. One per cache
    '
    '	1) set the primary content name in const cnPrimaryContent. avoid constants Like cnAddons used outside model
    '	2) find-And-replace "_blankModel" with the name for this model
    '	3) when adding model fields, add in three places: the Public Property, the saveObject(), the loadObject()
    '	4) when adding create() methods to support other fields/combinations of fields, 
    '       - add a secondary cache For that new create method argument in loadObjec()
    '       - add it to the injected cachename list in loadObject()
    '       - add an invalidate
    '
    ' Model Caching
    '   caching applies to model objects only, not lists of models (for now)
    '       - this is because of the challenge of invalidating the list object when individual records are added or deleted
    '
    '   a model should have 1 primary cache object which stores the data and can have other secondary cacheObjects which do not hold data
    '    the cacheName of the 'primary' cacheObject for models and db records (cacheNamePrefix + ".id." + #id)
    '    'secondary' cacheName is (cacheNamePrefix + . + fieldName + . + #)
    '
    '   cacheobjects can be used to hold data (primary cacheobjects), or to hold only metadata (secondary cacheobjects)
    '       - primary cacheobjects are like 'personModel.id.99' that holds the model for id=99
    '           - it is primary because the .primaryobject is null
    '           - invalidationData. This cacheobject is invalid after this datetime
    '           - dependentobjectlist() - this object is invalid if any of those objects are invalid
    '       - secondary cachobjects are like 'person.ccguid.12345678'. It does not hold data, just a reference to the primary cacheobject
    '
    '   cacheNames spaces are replaced with underscores, so "addon collections" should be addon_collections
    '
    '   cacheNames that match content names are treated as caches of "any" record in the content, so invalidating "people" can be used to invalidate
    '       any non-specific cache in the people table, by including "people" as a dependant cachename. the "people" cachename should not clear
    '       specific people caches, like people.id.99, but can be used to clear lists of records like "staff_list_group"
    '       - this can be used as a fallback strategy to cache record lists: a remote method list can be cached with a dependancy on "add-ons".
    '       - models should always clear this content name cache entry on all cache clears
    '
    '   when a model is created, the code first attempts to read the model's cacheobject. if it fails, it builds it and saves the cache object and tags
    '       - when building the model, is writes object to the primary cacheobject, and writes all the secondaries to be used
    '       - when building the model, if a database record is opened, a dependantObject Tag is created for the tablename+'id'+id
    '       - when building the model, if another model is added, that model returns its cachenames in the cacheNameList to be added as dependentObjects
    '
    '
    Public Class oldBaseModel
        '
        '-- const
        Public Const primaryContentName As String = "" '<------ set content name
        Public Const primaryContentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
        Public Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public Property id As Integer
        Public Property name As String
        Public Property ccguid As String
        Public Property Active As Boolean
        Public Property ContentControlID As Integer
        Public Property CreatedBy As Integer
        Public Property CreateKey As Integer
        Public Property DateAdded As Date
        Public Property ModifiedBy As Integer
        Public Property ModifiedDate As Date
        Public Property SortOrder As String
        '
        'Public foreignKey1Id As Integer ' <-- DELETE - sample field for create/delete patterns
        'Public foreignKey2Id As Integer ' <-- DELETE - sample field for create/delete patterns
        '
        ' -- publics not exposed to the UI (test/internal data)
        '<JsonIgnore> Public createKey As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
        ''' values in Contensive metadata (active, contentcontrolid, etc)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="callersCacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As oldBaseModel
            Dim result As oldBaseModel = Nothing
            Try
                result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, cpCore.doc.authContext.user.id), callersCacheNameList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected.
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="recordId"></param>
        ''' <returns></returns>
        Public Shared Function create(cpCore As coreClass, recordId As Integer) As oldBaseModel
            Return create(cpCore, recordId, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As oldBaseModel
            Dim result As oldBaseModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId)
                    result = cpCore.cache.getObject(Of oldBaseModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            Throw New ApplicationException("This cannot work - to get the derived class's primaryContentName, I need the 'me', but me not available because this is static.")
                            'If cs.open(Me.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public      instance.primaryContentName, "(id=" & recordId.ToString() & ")") Then
                            '    result = loadRecord(cpCore, cs, callersCacheNameList)
                            'End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As oldBaseModel
            Dim result As oldBaseModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of oldBaseModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' template for open an existing object with multiple keys (like a rule)
        '''' </summary>
        '''' <param name="cp"></param>
        '''' <param name="foreignKey1Id"></param>
        'Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As baseModel
        '    Dim result As baseModel = Nothing
        '    Try
        '        If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
        '            result = cpCore.cache.getObject(Of baseModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
        '            If (result Is Nothing) Then
        '                Using cs As New csController(cpCore)
        '                    If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
        '                        result = loadRecord(cpCore, cs, callersCacheNameList)
        '                    End If
        '                End Using
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '        Throw
        '    End Try
        '    Return result
        'End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordName"></param>
        Public Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As oldBaseModel
            Dim result As oldBaseModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordName) Then
                    Dim cacheName As String = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "name", recordName)
                    result = cpCore.cache.getObject(Of oldBaseModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(name=" & cpCore.db.encodeSQLText(recordName) & ")", "id") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As oldBaseModel
            Dim instance As oldBaseModel = Nothing
            Try
                If cs.ok() Then
                    instance = New oldBaseModel
                    For Each resultProperty As PropertyInfo In instance.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                        Select Case resultProperty.Name.ToLower()
                            Case "specialcasefield"
                            Case Else
                                Select Case resultProperty.PropertyType.Name
                                    Case "Int32"
                                        resultProperty.SetValue(instance, cs.getInteger(resultProperty.Name), Nothing)
                                    Case "Boolean"
                                        resultProperty.SetValue(instance, cs.getBoolean(resultProperty.Name), Nothing)
                                    Case "DateTime"
                                        resultProperty.SetValue(instance, cs.getDate(resultProperty.Name), Nothing)
                                    Case "Double"
                                        resultProperty.SetValue(instance, cs.getNumber(resultProperty.Name), Nothing)
                                    Case Else
                                        resultProperty.SetValue(instance, cs.getText(resultProperty.Name), Nothing)
                                End Select
                        End Select
                    Next
                    If (instance IsNot Nothing) Then
                        '
                        ' -- set primary cache to the object created
                        ' -- set secondary caches to the primary cache
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", instance.id.ToString())
                        callersCacheNameList.Add(cacheName0)
                        cpCore.cache.setContent(cacheName0, instance)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", instance.ccguid)
                        callersCacheNameList.Add(cacheName1)
                        cpCore.cache.setPointer(cacheName1, cacheName0)
                        '
                        'Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
                        'callersCacheNameList.Add(cacheName2)
                        'cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return instance
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <returns></returns>
        Public Function save(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (id > 0) Then
                    If Not cs.open(primaryContentName, "id=" & id) Then
                        Dim message As String = "Unable to open record in content [" & primaryContentName & "], with id [" & id & "]"
                        cs.Close()
                        id = 0
                        Throw New ApplicationException(message)
                    End If
                Else
                    If Not cs.Insert(primaryContentName) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
                    End If
                End If
                For Each resultProperty As PropertyInfo In Me.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                    Select Case resultProperty.Name.ToLower()
                        Case "id"
                            id = cs.getInteger("id")
                        Case "ccguid"
                            If (String.IsNullOrEmpty(ccguid)) Then
                                ccguid = Controllers.genericController.getGUID()
                            End If
                            Dim value As String
                            value = resultProperty.GetValue(Me, Nothing).ToString()
                            cs.setField(resultProperty.Name, value)
                        Case Else
                            Select Case resultProperty.PropertyType.Name
                                Case "Int32"
                                    Dim value As Integer
                                    Integer.TryParse(resultProperty.GetValue(Me, Nothing).ToString(), value)
                                    cs.setField(resultProperty.Name, value)
                                Case "Boolean"
                                    Dim value As Boolean
                                    Boolean.TryParse(resultProperty.GetValue(Me, Nothing).ToString(), value)
                                    cs.setField(resultProperty.Name, value)
                                Case "DateTime"
                                    Dim value As Date
                                    Date.TryParse(resultProperty.GetValue(Me, Nothing).ToString(), value)
                                    cs.setField(resultProperty.Name, value)
                                Case "Double"
                                    Dim value As Double
                                    Double.TryParse(resultProperty.GetValue(Me, Nothing).ToString(), value)
                                    cs.setField(resultProperty.Name, value)
                                Case Else
                                    Dim value As String
                                    value = resultProperty.GetValue(Me, Nothing).ToString()
                                    cs.setField(resultProperty.Name, value)
                            End Select
                    End Select
                Next
                '
                ' -- invalidate objects
                ' -- no, the primary is invalidated by the cs.save()
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
                ' -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
                '
                ' -- object is here, but the cache was invalidated, setting
                cpCore.cache.setContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", Me.id.ToString()), Me)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
                    cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId))
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by guid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccguid"></param>
        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    Dim instance As oldBaseModel = create(cpCore, ccguid, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.id)
                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' pattern to delete an existing object based on multiple criteria (like a rule record)
        '''' </summary>
        '''' <param name="cp"></param>
        '''' <param name="foreignKey1Id"></param>
        '''' <param name="foreignKey2Id"></param>
        'Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
        '    Try
        '        If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
        '            Dim instance As baseModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
        '            If (instance IsNot Nothing) Then
        '                invalidatePrimaryCache(cpCore, instance.id)
        '                cpCore.db.deleteTableRecord(primaryContentTableName, instance.id, primaryContentDataSource)
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '        Throw
        '    End Try
        'End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList(cpCore As coreClass, sqlCriteria As String, callersCacheNameList As List(Of String)) As List(Of oldBaseModel)
            Dim result As New List(Of oldBaseModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, sqlCriteria, "id")) Then
                    Dim instance As oldBaseModel
                    Do
                        instance = loadRecord(cpCore, cs, callersCacheNameList)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate the primary key (which depends on all secondary keys)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", "0"))
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' produce a standard format cachename for this model
        '''' </summary>
        '''' <param name="fieldName"></param>
        '''' <param name="fieldValue"></param>
        '''' <returns></returns>
        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
        '    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        'End Function
        ''
        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
        '    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        'End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return oldBaseModel.create(cpcore, recordId, New List(Of String)).name
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return oldBaseModel.create(cpcore, ccGuid, New List(Of String)).name
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the id of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return oldBaseModel.create(cpcore, ccGuid, New List(Of String)).id
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function createDefault(cpcore As coreClass) As oldBaseModel
            Dim instance As New oldBaseModel
            Try
                Dim CDef As Models.Complex.cdefModel = Models.Complex.cdefModel.getCdef(cpcore, primaryContentName)
                If (CDef Is Nothing) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                ElseIf (CDef.Id <= 0) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                Else
                    With CDef
                        For Each resultProperty As PropertyInfo In instance.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                            Select Case resultProperty.Name.ToLower()
                                Case "id"
                                    instance.id = 0
                                Case Else
                                    Select Case resultProperty.PropertyType.Name
                                        Case "Int32"
                                            resultProperty.SetValue(instance, genericController.EncodeInteger(.fields(resultProperty.Name).defaultValue), Nothing)
                                        Case "Boolean"
                                            resultProperty.SetValue(instance, genericController.EncodeBoolean(.fields(resultProperty.Name).defaultValue), Nothing)
                                        Case "DateTime"
                                            resultProperty.SetValue(instance, genericController.EncodeDate(.fields(resultProperty.Name).defaultValue), Nothing)
                                        Case "Double"
                                            resultProperty.SetValue(instance, genericController.EncodeNumber(.fields(resultProperty.Name).defaultValue), Nothing)
                                        Case Else
                                            resultProperty.SetValue(instance, .fields(resultProperty.Name).defaultValue, Nothing)
                                    End Select
                            End Select
                        Next
                    End With
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return instance
        End Function
    End Class
End Namespace
