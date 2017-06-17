
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
    Public MustInherit Class baseModel
        '
        '====================================================================================================
        '-- const must be set in derived clases
        '
        'Public Const contentName As String = "" '<------ set content name
        'Public Const contentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
        'Public Const contentDataSource As String = "" '<----- set to datasource if not default
        '
        '====================================================================================================
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
        '====================================================================================================
        Private Shared Function derivedContentName(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
        '
        '====================================================================================================
        Private Shared Function derivedContentTableName(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentTableName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentTableName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
        '
        '====================================================================================================
        Private Shared Function contentDataSource(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentTableName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentTableName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
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
        ''' Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="cpCore"></param>
        ''' <returns></returns>
        Protected Shared Function add(Of T)(cpCore As coreClass) As T
            Return add(Of T)(cpCore, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        ''' include callersCacheNameList to get a list of cacheNames used to assemble this response
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="callersCacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(Of T)(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As T
            Dim result As T = Nothing
            Try
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                result = create(Of T)(cpCore, cpCore.db.metaData_InsertContentRecordGetID(contentName, cpCore.authContext.user.ID), callersCacheNameList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Protected Shared Function create(Of T)(cpCore As coreClass, recordId As Integer) As T
            Return create(Of T)(cpCore, recordId, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(Of T)(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As T
            Dim result As T = Nothing
            Try
                If recordId > 0 Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString())
                    result = cpCore.cache.getObject(Of T)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(contentName, "(id=" & recordId.ToString() & ")") Then
                                result = loadRecord(Of T)(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Shared Function create(Of T)(cpCore As coreClass, recordGuid As String) As T
            Return create(Of T)(cpCore, recordGuid, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function create(Of T)(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As T
            Dim result As T = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of T)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(contentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
                                result = loadRecord(Of T)(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Shared Function createByName(Of T)(cpCore As coreClass, recordName As String) As T
            Return createByName(Of T)(cpCore, recordName, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordName"></param>
        Public Shared Function createByName(Of T)(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As T
            Dim result As T = Nothing
            Try
                If Not String.IsNullOrEmpty(recordName) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "name", recordName)
                    result = cpCore.cache.getObject(Of T)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(contentName, "(name=" & cpCore.db.encodeSQLText(recordName) & ")", "id") Then
                                result = loadRecord(Of T)(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Private Shared Function loadRecord(Of T)(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As T
            Dim instance As T = Nothing
            Try
                If cs.ok() Then
                    Dim instanceType As Type = GetType(T)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    instance = DirectCast(Activator.CreateInstance(instanceType), T)
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
                        Dim baseInstance As baseModel = TryCast(instance, baseModel)
                        If (baseInstance IsNot Nothing) Then
                            Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(tableName, "id", baseInstance.id.ToString())
                            callersCacheNameList.Add(cacheName0)
                            cpCore.cache.setObject(cacheName0, instance)
                            '
                            Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(tableName, "ccguid", baseInstance.ccguid)
                            callersCacheNameList.Add(cacheName1)
                            cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim instanceType As Type = Me.GetType()
                Dim contentName As String = derivedContentName(instanceType)
                Dim tableName As String = derivedContentTableName(instanceType)
                If (id > 0) Then
                    If Not cs.open(contentName, "id=" & id) Then
                        Dim message As String = "Unable to open record in content [" & contentName & "], with id [" & id & "]"
                        cs.Close()
                        id = 0
                        Throw New ApplicationException(message)
                    End If
                Else
                    If Not cs.Insert(contentName) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & contentName & "]")
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
                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", Me.id.ToString()), Me)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Shared Sub delete(Of T)(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    cpCore.db.deleteContentRecords(contentName, "id=" & recordId.ToString)
                    cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Shared Sub delete(Of T)(cpCore As coreClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim instance As baseModel = create(Of baseModel)(cpCore, ccguid)
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(Of T)(cpCore, instance.id)
                        cpCore.db.deleteContentRecords(contentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        '            Dim instance As baseModel2 = create(Of baseModel2)(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
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
        Public Shared Function createList(Of T)(cpCore As coreClass, sqlCriteria As String) As List(Of T)
            Return createList(Of T)(cpCore, sqlCriteria, New List(Of String))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList(Of T)(cpCore As coreClass, sqlCriteria As String, callersCacheNameList As List(Of String)) As List(Of T)
            Dim result As New List(Of T)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                If (cs.open(contentName, sqlCriteria, "id")) Then
                    Dim instance As T
                    Do
                        instance = loadRecord(Of T)(cpCore, cs, callersCacheNameList)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Shared Sub invalidatePrimaryCache(Of T)(cpCore As coreClass, recordId As Integer)
            Dim instanceType = GetType(T)
            Dim tableName As String = derivedContentTableName(instanceType)
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", "0"))
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
        Public Shared Function getRecordName(Of T)(cpcore As coreClass, recordId As Integer) As String
            If (recordId > 0) Then
                Dim instanceType As Type = GetType(T)
                Dim tableName As String = derivedContentTableName(instanceType)
                Using cs As New csController(cpcore)
                    If (cs.openSQL("select name from " & tableName & " where id=" & recordId.ToString())) Then
                        Return cs.getText("name")
                    End If
                End Using
            End If
            Return ""
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(Of T)(cpcore As coreClass, ccGuid As String) As String
            If (Not String.IsNullOrEmpty(ccGuid)) Then
                Dim instanceType As Type = GetType(T)
                Dim tableName As String = derivedContentTableName(instanceType)
                Using cs As New csController(cpcore)
                    If (cs.openSQL("select name from " & tableName & " where ccguid=" & cpcore.db.encodeSQLText(ccGuid))) Then
                        Return cs.getText("name")
                    End If
                End Using
            End If
            Return ""
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the id of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordId(Of T)(cpcore As coreClass, ccGuid As String) As Integer
            If (Not String.IsNullOrEmpty(ccGuid)) Then
                Dim instanceType As Type = GetType(T)
                Dim tableName As String = derivedContentTableName(instanceType)
                Using cs As New csController(cpcore)
                    If (cs.openSQL("select id from " & tableName & " where ccguid=" & cpcore.db.encodeSQLText(ccGuid))) Then
                        Return cs.getInteger("id")
                    End If
                End Using
            End If
            Return 0
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function createDefault(Of T)(cpcore As coreClass) As T
            Dim instanceType As Type = GetType(T)
            Dim instance As T = DirectCast(Activator.CreateInstance(instanceType), T)
            Try
                Dim contentName As String = derivedContentName(instanceType)
                Dim CDef As cdefModel = cpcore.metaData.getCdef(contentName)
                If (CDef Is Nothing) Then
                    Throw New ApplicationException("content [" & contentName & "] could Not be found.")
                ElseIf (CDef.Id <= 0) Then
                    Throw New ApplicationException("content [" & contentName & "] could Not be found.")
                Else
                    With CDef
                        For Each resultProperty As PropertyInfo In instance.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                            Select Case resultProperty.Name.ToLower()
                                Case "id"
                                    resultProperty.SetValue(instance, 0)
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
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return instance
        End Function
    End Class
End Namespace
