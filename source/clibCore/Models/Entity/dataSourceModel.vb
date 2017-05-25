
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class dataSourceModel
        '
        '-- const
        Public Const primaryContentName As String = "data sources"
        Private Const primaryContentTableName As String = "ccDataSources"
        Private Const primaryContentDataSource As String = "default"
        '
        Public Enum dataSourceTypeEnum
            sqlServerOdbc = 1
            sqlServerNative = 2
            mySqlNative = 3
        End Enum
        '
        ' -- instance properties
        Public ID As Integer
        Public Active As Boolean
        Public ccGuid As String
        Public ConnString As String
        Public endPoint As String
        Public username As String
        Public password As String
        Public type As Integer
        Public ContentCategoryID As Integer
        Public ContentControlID As Integer
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public DateAdded As Date
        Public EditArchive As Boolean
        Public EditBlank As Boolean
        Public EditSourceID As Integer
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public SortOrder As String
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
        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.authContext.user.ID), callersCacheNameList)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                If recordId <= 0 Then
                    result = getDefaultDatasource(cpCore)
                Else
                    Dim cacheName As String = GetType(dataSourceModel).FullName & getCacheName("id", recordId.ToString())
                    result = cpCore.cache.getObject(Of dataSourceModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "id=" & recordId.ToString(), callersCacheNameList)
                    End If
                End If

            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                If String.IsNullOrEmpty(recordGuid) Then
                    result = getDefaultDatasource(cpCore)
                Else
                    Dim cacheName As String = GetType(dataSourceModel).FullName & getCacheName("ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of dataSourceModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid), callersCacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' template for open an existing object with multiple keys (like a rule)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="foreignKey1Id"></param>
        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
                    result = cpCore.cache.getObject(Of dataSourceModel)(getCacheName("foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")", callersCacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New dataSourceModel
                    With result
                        '
                        ' -- populate result model
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .ccGuid = cs.getText("ccGuid")
                        .ConnString = cs.getText("ConnString")
                        .endPoint = cs.getText("endpoint")
                        .username = cs.getText("username")
                        .password = cs.getText("password")
                        .type = cs.getInteger("dbTypeId")
                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        .EditArchive = cs.getBoolean("EditArchive")
                        .EditBlank = cs.getBoolean("EditBlank")
                        .EditSourceID = cs.getInteger("EditSourceID")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = normalizeDataSourceName(cs.getText("Name"))
                        .SortOrder = cs.getText("SortOrder")
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary cache to the object created
                        ' -- set secondary caches to the primary cache
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = getCacheName("id", result.ID.ToString())
                        callersCacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = getCacheName("ccguid", result.ccGuid)
                        callersCacheNameList.Add(cacheName1)
                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
                    End If
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <returns></returns>
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (ID > 0) Then
                    If Not cs.open(primaryContentName, "id=" & ID) Then
                        ID = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record in content [" & primaryContentName & "], with id [" & ID & "]")
                    End If
                Else
                    If Not cs.Insert(primaryContentName) Then
                        cs.Close()
                        ID = 0
                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
                    End If
                End If
                If cs.ok() Then
                    ID = cs.getInteger("id")
                    cs.setField("Active", Active.ToString())
                    cs.setField("ccGuid", ccGuid)
                    cs.setField("ConnString", ConnString)
                    cs.setField("endPoint", endPoint)
                    cs.setField("username", username)
                    cs.setField("password", password)
                    cs.setField("dbTypeId", type)
                    cs.setField("ContentCategoryID", ContentCategoryID.ToString())
                    cs.setField("ContentControlID", ContentControlID.ToString())
                    cs.setField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.setField("DateAdded", DateAdded.ToString())
                    cs.setField("EditArchive", EditArchive.ToString())
                    cs.setField("EditBlank", EditBlank.ToString())
                    cs.setField("EditSourceID", EditSourceID.ToString())
                    cs.setField("ModifiedBy", ModifiedBy.ToString())
                    cs.setField("ModifiedDate", ModifiedDate.ToString())
                    cs.setField("Name", normalizeDataSourceName(Name))
                    cs.setField("SortOrder", SortOrder)
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                cpCore.cache.invalidateObject(getCacheName("id", ID.ToString))
                cpCore.cache.invalidateObject(getCacheName("ccguid", ccGuid))
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return ID
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
                    cpCore.cache.invalidateObject(getCacheName("id", recordId.ToString))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                    Dim instance As dataSourceModel = create(cpCore, ccguid, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern to delete an existing object based on multiple criteria (like a rule record)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="foreignKey1Id"></param>
        ''' <param name="foreignKey2Id"></param>
        Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
            Try
                If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
                    Dim instance As dataSourceModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.ID, primaryContentDataSource)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer, callersCacheNameList As List(Of String)) As List(Of dataSourceModel)
            Dim result As New List(Of dataSourceModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As dataSourceModel
                    Do
                        instance = dataSourceModel.create(cpCore, cs.getInteger("id"), callersCacheNameList)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
            cpCore.cache.invalidateObject(getCacheName("id", recordId.ToString))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateObject(getCacheName("id", "0"))
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' produce a standard format cachename for this model
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <param name="fieldValue"></param>
        ''' <returns></returns>
        Private Shared Function getCacheName(fieldName As String, fieldValue As String) As String
            Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        '
        Private Shared Function getCacheName(field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
            Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return normalizeDataSourceName(dataSourceModel.create(cpcore, recordId, New List(Of String)).Name)
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
            Return normalizeDataSourceName(dataSourceModel.create(cpcore, ccGuid, New List(Of String)).Name)
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
            Return dataSourceModel.create(cpcore, ccGuid, New List(Of String)).ID
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a datasource name into the key value used by the datasourcedictionary cache
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <returns></returns>
        Public Shared Function normalizeDataSourceName(DataSourceName As String) As String
            If Not String.IsNullOrEmpty(DataSourceName) Then
                Return DataSourceName.Trim().ToLower()
            End If
            Return String.Empty
        End Function
        '
        '====================================================================================================
        Public Shared Function getNameDict(cpcore As coreClass) As Dictionary(Of String, dataSourceModel)
            Dim result As New Dictionary(Of String, dataSourceModel)
            Try
                Dim cs As New csController(cpcore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "", "id", True, "id")) Then
                    Do
                        Dim instance As dataSourceModel = create(cpcore, cs.getInteger("id"), New List(Of String))
                        If (instance IsNot Nothing) Then
                            result.Add(instance.Name.ToLower(), instance)
                        End If
                    Loop
                End If
                If (Not result.ContainsKey("default")) Then
                    result.Add("default", getDefaultDatasource(cpcore))
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object from its name
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                If (String.IsNullOrEmpty(recordName.Trim()) Or (recordName.Trim.ToLower() = "default")) Then
                    result = getDefaultDatasource(cpCore)
                Else
                    Dim cacheName As String = GetType(dataSourceModel).FullName & getCacheName("ccguid", recordName)
                    result = cpCore.cache.getObject(Of dataSourceModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "name=" & cpCore.db.encodeSQLText(recordName), callersCacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return the default datasource
        ''' </summary>
        ''' <param name="cp"></param>
        Public Shared Function getDefaultDatasource(cpCore As coreClass) As dataSourceModel
            Dim result As dataSourceModel = Nothing
            Try
                result = New dataSourceModel
                With result
                    .Active = True
                    .ccGuid = ""
                    .ConnString = ""
                    .ContentCategoryID = 0
                    .ContentControlID = 0
                    .CreatedBy = 0
                    .CreateKey = 0
                    .DateAdded = Date.MinValue
                    .type = dataSourceTypeEnum.sqlServerNative
                    .endPoint = cpCore.serverConfig.defaultDataSourceAddress
                    .Name = "default"
                    .password = cpCore.serverConfig.password
                    .username = cpCore.serverConfig.username
                End With
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        ''
        ''====================================================================================================
        'Public Shared Function getHtmlInputSelect(cpCore As coreClass, HtmlName As String, selectedId As Integer, callerCacheList As List(Of String)) As String
        '    Dim result As String = ""
        '    Try
        '        result = cpCore.htmlDoc.main_GetFormInputSelect2(HtmlName, selectedId, primaryContentName, "", "default", "", Nothing)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex)
        '    End Try
        '    Return result
        'End Function
    End Class
End Namespace
