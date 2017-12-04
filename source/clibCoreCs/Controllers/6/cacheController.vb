
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
Imports System.Runtime.Caching
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' Interface to cache systems. Cache objects are saved to dotnet cache, remotecache, filecache. 
    ''' 
    ''' 3 types of key
    ''' -- 1) primary key -- cache key holding the content.
    ''' -- 2) dependent key -- a dependent key holds content that the primary cache depends on. If the dependent cache is invalid, the primary cache is invalid
    ''' -------- if an org cache includes a primary content person, then the org (org/id/10) is saved with a dependent key (ccmembers/id/99). The org content id valid only if both the primary and dependent keys are valid
    ''' -- 3) pointer key -- a cache entry that holds a primary key (or another pointer key). When read, the cache returns the primary value. (primary="ccmembers/id/10", pointer="ccmembers/ccguid/{1234}"
    ''' ------- pointer keys are read-only, as the content is saved in the primary key
    ''' 
    ''' three  types of cache entries:
    '''  -- simple entity cache -- cache of an entity model (one record)
    '''     .. saveCache, reachCache in the entity model
    '''     .. invalidateCache in the entity model, includes invalication for complex objects that include the entity
    '''      
    '''  -- complex entity cache -- cache of an object with mixed data based on the entity data model (an org object that contains a person object)
    '''    .. has an id for the topmost object
    '''    .. these objects are in .. ? (another model folder?)
    '''    
    ''' When a record is saved in admin, an invalidation call is made for tableName/id/#
    ''' 
    ''' If code edits a db record, you should call invalidate for tablename/id/#
    ''' 
    ''' one-to-many lists: if the list is cached, it has to include dependent keys for all its included members
    ''' 
    ''' many-to-many lists: should have a model for the rule table, and lists should come from methods there. the delete method must invalidate the rule model cache
    ''' 
    ''' A cacheobject has:
    '''   key - if the object is a model of a database record, and there is only one model for that db record, use tableName+id as the cacheName. If it contains other data,
    '''     use the modelName+id as the cacheName, and include all records in the dependentObjectList.
    '''   primaryObjectKey -- a secondary cacheObject does not hold data, just a pointer to a primary object. For example, if you use both id and guid to reference objects,
    '''     save the data in the primary cacheObject (tableName+id) and save a secondary cacheObject (tablename+guid). Sve both when you update the cacheObject. requesting either
    '''     or invalidating either will effect the primary cache
    '''   dependentObjectList -- if a cacheobject contains data from multiple sources, each source should be included in the dependencyList. This includes both cached models and Db records.
    '''   
    ''' Special cache entries
    '''     dbTablename - cachename = the name of the table
    '''         - when any record is saved to the table, this dbTablename cache is updated
    '''         - objects like addonList depend on it, and are flushed if ANY record in that table is updated
    '''         
    ''' </summary>
    Public Class cacheController
        Implements IDisposable
        '
        ' ----- constants
        '
        Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        ' ----- objects constructed that must be disposed
        '
        Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- private instance storage
        '
        Private remoteCacheInitialized As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' cache data wrapper to include tags and save datetime
        ''' </summary>
        <Serializable()>
        Public Class cacheWrapperClass
            Public key As String                                    ' if populated, all other properties are ignored and the primary tag b
            Public dependentKeyList As New List(Of String)          ' this object is invalidated if any of these objects are invalidated
            Public saveDate As Date                                 ' the date this object was last saved.
            Public invalidationDate As Date                         ' the future date when this object self-invalidates
            Public content As Object                                ' the data storage
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared. Every cache object saved before this date is considered invalid.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property globalInvalidationDate As Date
            Get
                '
                If (_globalInvalidationDate Is Nothing) Then
                    Dim dataObject As cacheWrapperClass = getWrappedContent("globalInvalidationDate")
                    If (dataObject IsNot Nothing) Then
                        _globalInvalidationDate = dataObject.saveDate
                    End If
                    If (_globalInvalidationDate Is Nothing) Then
                        _globalInvalidationDate = Now()
                        setWrappedContent("globalInvalidationDate", New cacheWrapperClass With {.saveDate = CDate(_globalInvalidationDate)})
                    End If
                End If
                If (Not _globalInvalidationDate.HasValue) Then
                    _globalInvalidationDate = Now.AddDays(invalidationDaysDefault)
                Else
                    If ((CDate(_globalInvalidationDate)).CompareTo(New Date(1990, 8, 7)) < 0) Then
                        _globalInvalidationDate = New Date(1990, 8, 7)
                    End If
                End If
                Return CDate(_globalInvalidationDate)
            End Get
        End Property
        Private _globalInvalidationDate As Date?
        '
        '====================================================================================================
        ''' <summary>
        ''' Initializes cache client
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            Try
                Me.cpCore = cpCore
                '
                _globalInvalidationDate = Nothing
                remoteCacheInitialized = False
                If (cpCore.serverConfig.enableRemoteCache) Then
                    Dim cacheEndpoint As String = cpCore.serverConfig.awsElastiCacheConfigurationEndpoint
                    If Not String.IsNullOrEmpty(cacheEndpoint) Then
                        Dim cacheEndpointSplit As String() = cacheEndpoint.Split(":"c)
                        Dim cacheEndpointPort As Integer = 11211
                        If cacheEndpointSplit.Count > 1 Then
                            cacheEndpointPort = genericController.EncodeInteger(cacheEndpointSplit(1))
                        End If
                        Dim cacheConfig As Amazon.ElastiCacheCluster.ElastiCacheClusterConfig = New Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(cacheEndpointSplit(0), cacheEndpointPort)
                        cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary
                        cacheClient = New Enyim.Caching.MemcachedClient(cacheConfig)
                        If (cacheClient IsNot Nothing) Then remoteCacheInitialized = True
                    End If
                End If
            Catch ex As Exception
                '
                ' -- client does not throw its own errors, so try to differentiate by message
                Throw (New ApplicationException("Exception initializing remote cache, will continue with cache disabled.", ex))
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save object directly to cache.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="wrappedContent">Either a string, a date, or a serializable object</param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub setWrappedContent(ByVal key As String, ByVal wrappedContent As cacheWrapperClass)
            Try
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("cache key cannot be blank")
                Else
                    Dim wrapperKey As String = getWrapperKey(cpCore.serverConfig.appConfig.name, key)
                    If (cpCore.serverConfig.enableRemoteCache) Then
                        If (remoteCacheInitialized) Then
                            '
                            ' -- save remote cache
                            Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, wrapperKey, wrappedContent, wrappedContent.invalidationDate)
                        End If
                        If cpCore.serverConfig.enableLocalMemoryCache Then
                            '
                            ' -- save local memory cache
                            setWrappedContent_MemoryCache(wrapperKey, wrappedContent)
                        End If
                        If cpCore.serverConfig.enableLocalFileCache Then
                            '
                            ' -- save local memory cache
                            Dim serializedData As String = Newtonsoft.Json.JsonConvert.SerializeObject(wrappedContent)
                            Using mutex As New System.Threading.Mutex(False, wrapperKey)
                                mutex.WaitOne()
                                cpCore.privateFiles.saveFile("appCache\" & genericController.encodeFilename(wrapperKey & ".txt"), serializedData)
                                mutex.ReleaseMutex()
                            End Using
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save cacheWrapper to memory cache
        ''' </summary>
        ''' <param name="wrapperKey"></param>
        ''' <param name="wrappedContent"></param>
        Public Sub setWrappedContent_MemoryCache(ByVal wrapperKey As String, ByVal wrappedContent As cacheWrapperClass)
            Dim cache As ObjectCache = MemoryCache.Default
            Dim policy As New CacheItemPolicy()
            policy.AbsoluteExpiration = Now.AddMinutes(100)
            cache.Set(wrapperKey, wrappedContent, policy)
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' get an object from cache. If the cache misses or is invalidated, nothing object is returned
        ''' </summary>
        ''' <typeparam name="objectClass"></typeparam>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function getObject(Of objectClass)(ByVal key As String) As objectClass
            Dim result As objectClass = Nothing
            Try
                Dim wrappedContent As cacheWrapperClass
                Dim cacheMiss As Boolean = False
                Dim dateCompare As Integer
                '
                If Not (String.IsNullOrEmpty(key)) Then
                    '
                    ' -- read cacheWrapper
                    appendCacheLog("getObject(" & key & "), enter") : Dim sw As Stopwatch = Stopwatch.StartNew()
                    wrappedContent = getWrappedContent(key)
                    If (wrappedContent IsNot Nothing) Then
                        '
                        ' -- test for global invalidation
                        dateCompare = globalInvalidationDate.CompareTo(wrappedContent.saveDate)
                        If (dateCompare >= 0) Then
                            '
                            ' -- global invalidation
                            appendCacheLog("GetObject(" & key & "), invalidated because the cacheObject's saveDate [" & wrappedContent.saveDate.ToString() & "] is after the globalInvalidationDate [" & globalInvalidationDate & "]")
                        Else
                            '
                            ' -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
                            If wrappedContent.dependentKeyList.Count = 0 Then
                                '
                                ' -- no dependent objects
                                appendCacheLog("GetObject(" & key & "), no dependentKeyList")
                            Else
                                For Each dependentKey As String In wrappedContent.dependentKeyList
                                    Dim dependantObject As cacheWrapperClass = getWrappedContent(dependentKey)
                                    If (dependantObject IsNot Nothing) Then
                                        dateCompare = dependantObject.saveDate.CompareTo(wrappedContent.saveDate)
                                        If (dateCompare >= 0) Then
                                            cacheMiss = True
                                            appendCacheLog("GetObject(" & key & "), invalidated because the dependantobject [" & dependentKey & "] has a saveDate [" & dependantObject.saveDate.ToString() & "] after the cacheObject's dateDate [" & wrappedContent.saveDate.ToString() & "]")
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                            If Not cacheMiss Then
                                If (Not String.IsNullOrEmpty(wrappedContent.key)) Then
                                    '
                                    ' -- this is a pointer key, load the primary
                                    result = getObject(Of objectClass)(wrappedContent.key)
                                ElseIf (TypeOf wrappedContent.content Is Newtonsoft.Json.Linq.JObject) Then
                                    '
                                    ' -- newtonsoft types
                                    Dim data As Newtonsoft.Json.Linq.JObject = DirectCast(wrappedContent.content, Newtonsoft.Json.Linq.JObject)
                                    result = data.ToObject(Of objectClass)()
                                ElseIf (TypeOf wrappedContent.content Is Newtonsoft.Json.Linq.JArray) Then
                                    '
                                    ' -- newtonsoft types
                                    Dim data As Newtonsoft.Json.Linq.JArray = DirectCast(wrappedContent.content, Newtonsoft.Json.Linq.JArray)
                                    result = data.ToObject(Of objectClass)()
                                ElseIf (TypeOf wrappedContent.content Is String) And (TypeOf result IsNot String) Then
                                    '
                                    ' -- if cache data was left as a string (might be empty), and return object is not string, there was an error
                                    result = Nothing
                                Else
                                    '
                                    ' -- all worked
                                    result = DirectCast(wrappedContent.content, objectClass)
                                End If
                            End If
                        End If
                    End If
                    appendCacheLog("getObject(" & key & "), exit(" & sw.ElapsedMilliseconds & "ms)") : sw.Stop()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a cache object from the cache. returns the cacheObject that wraps the object
        ''' </summary>
        ''' <typeparam name="returnType"></typeparam>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Private Function getWrappedContent(ByVal key As String) As cacheWrapperClass
            Dim result As cacheWrapperClass = Nothing
            Try
                appendCacheLog("getWrappedContent(" & key & "), enter ")
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("key cannot be blank")
                Else
                    Dim wrapperKey As String = getWrapperKey(cpCore.serverConfig.appConfig.name, key)
                    If (remoteCacheInitialized) Then
                        '
                        ' -- use remote cache
                        Try
                            result = cacheClient.Get(Of cacheWrapperClass)(wrapperKey)
                        Catch ex As Exception
                            '
                            ' --client does not throw its own errors, so try to differentiate by message
                            If (ex.Message.ToLower.IndexOf("unable to load type") >= 0) Then
                                '
                                ' -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                cacheClient.Remove(wrapperKey)
                                result = Nothing
                            Else
                                '
                                ' -- some other error
                                cpCore.handleException(ex) : Throw
                            End If
                        End Try
                        If (result IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & key & "), remoteCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & key & "), remoteCache miss")
                        End If
                    End If
                    If (result Is Nothing) And cpCore.serverConfig.enableLocalMemoryCache Then
                        '
                        ' -- local memory cache
                        'Dim cache As ObjectCache = MemoryCache.Default
                        result = DirectCast(MemoryCache.Default(wrapperKey), cacheWrapperClass)
                        If (result IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & key & "), memoryCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & key & "), memoryCache miss")
                        End If
                    End If
                    If (result Is Nothing) And cpCore.serverConfig.enableLocalFileCache Then
                        '
                        ' -- local file cache
                        Dim serializedDataObject As String = Nothing
                        Using mutex As New System.Threading.Mutex(False, wrapperKey)
                            mutex.WaitOne()
                            serializedDataObject = cpCore.privateFiles.readFile("appCache\" & genericController.encodeFilename(wrapperKey & ".txt"))
                            mutex.ReleaseMutex()
                        End Using
                        If String.IsNullOrEmpty(serializedDataObject) Then
                            appendCacheLog("getCacheWrapper(" & key & "), file miss")
                        Else
                            appendCacheLog("getCacheWrapper(" & key & "), file hit, write to memory cache")
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject(Of cacheWrapperClass)(serializedDataObject)
                            setWrappedContent_MemoryCache(wrapperKey, result)
                        End If
                        If (result IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & key & "), fileCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & key & "), fileCache miss")
                        End If
                    End If
                    If (result IsNot Nothing) Then
                        '
                        ' -- empty objects return nothing, empty lists return count=0
                        If (result.dependentKeyList Is Nothing) Then
                            result.dependentKeyList = New List(Of String)
                        End If
                    End If
                End If
                appendCacheLog("getCacheWrapper(" & key & "), exit ")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save a string to cache, for compatibility with existing site. (no objects, no invalidation, yet)
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="content"></param>
        ''' <remarks></remarks>
        Public Sub setContent(key As String, content As Object)
            Try
                Dim wrappedContent As New cacheWrapperClass With {
                    .content = content,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .dependentKeyList = Nothing
                }
                setWrappedContent(key, wrappedContent)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save an object to cache, with invalidation
        ''' 
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="content"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="dependentKeyList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setContent(key As String, content As Object, invalidationDate As Date, dependentKeyList As List(Of String))
            Try
                Dim wrappedContent As New cacheWrapperClass With {
                    .content = content,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = invalidationDate,
                    .dependentKeyList = dependentKeyList
                }
                setWrappedContent(key, wrappedContent)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save an object to cache, with invalidation
        ''' 
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="content"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="dependantKey">The cache name of an object that .</param>
        ''' <remarks></remarks>
        Public Sub setContent(key As String, content As Object, invalidationDate As Date, dependantKey As String)
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .content = content,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = invalidationDate,
                    .dependentKeyList = New List(Of String)({dependantKey})
                }
                setWrappedContent(key, cacheWrapper)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value, compatible with legacy method signature.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="key"></param>
        ''' <param name="content"></param>
        ''' <param name="dependentKeyList">List of dependent keys.</param>
        ''' <remarks>If a dependent key is invalidated, it's parent key is also invalid. 
        ''' ex - org/id/10 has primary contact person/id/99. if org/id/10 object includes person/id/99 object, then org/id/10 depends on person/id/99,
        ''' and "person/id/99" is a dependent key for "org/id/10". When "org/id/10" is read, it checks all its dependent keys (person/id/99) and
        ''' invalidates if any dependent key is invalid.</remarks>
        Public Sub setContent(key As String, content As Object, dependentKeyList As List(Of String))
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .content = content,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .dependentKeyList = dependentKeyList
                }
                setWrappedContent(key, cacheWrapper)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value, compatible with legacy method signature.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="key"></param>
        ''' <param name="content"></param>
        ''' <param name="dependantKey"></param>
        ''' <remarks></remarks>
        Public Sub setContent(key As String, content As Object, dependantKey As String)
            Try
                'Dim dependantObjectList As New List(Of String)
                'dependantObjectList.Add(dependantObject)
                setContent(key, content, New List(Of String)({dependantKey}))
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' set a pointer key. A pointer key just points to a cache key, creating an altername way to get/invalidate a cache.
        ''' ex - image with id=10, guid={999}. The cache key="image/id/10", the pointerKey="image/ccguid/{9999}"
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="pointerKey"></param>
        ''' <param name="data"></param>
        ''' <param name="dependantObject"></param>
        ''' <remarks></remarks>
        Public Sub setPointer(pointerKey As String, key As String)
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .key = key
                }
                setWrappedContent(pointerKey, cacheWrapper)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates the entire cache (except those entires written with saveRaw)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub invalidateAll()
            Try
                Call setWrappedContent("globalInvalidationDate", New cacheWrapperClass With {.saveDate = Now()})
                _globalInvalidationDate = Nothing
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' invalidates a tag
        '''' </summary>
        '''' <param name="tag"></param>
        '''' <remarks></remarks>
        Public Sub invalidateContent(ByVal key As String)
            Try
                If Not String.IsNullOrEmpty(key.Trim()) Then
                    setWrappedContent(key, New cacheWrapperClass With {.saveDate = Now()})
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' update the dbTablename cache entry. Do this anytime any record is updated in the table
        ''' </summary>
        ''' <param name="ContentName"></param>
        Public Sub invalidateAllObjectsInContent(ByVal ContentName As String)
            Try
                invalidateAllObjectsInTable(Models.Complex.cdefModel.getContentTablename(cpCore, ContentName))
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' invalidate all cache entries to this table. "dbTableName"
        ''' when any cache is saved, it should include a dependancy on a cachename=dbtablename
        ''' </summary>
        ''' <param name="dbTableName"></param>
        Public Sub invalidateAllObjectsInTable(ByVal dbTableName As String)
            Try
                invalidateContent(dbTableName.ToLower().Replace(" ", "_"))
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a list of tags 
        ''' </summary>
        ''' <param name="keyList"></param>
        ''' <remarks></remarks>
        Public Sub invalidateContent(ByVal keyList As List(Of String))
            Try
                For Each key In keyList
                    Call invalidateContent(key)
                Next
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=======================================================================
        ''' <summary>
        ''' Encode a string to be memCacheD compatible, removing 0x00-0x20 and space
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getWrapperKey(appName As String, key As String) As String
            Dim result As String
            result = appName & "-" & key
            result = Regex.Replace(result, "0x[a-fA-F\d]{2}", "_")
            result = result.Replace(" ", "_")
            '
            ' -- 20170515 JK tmp - makes it harder to debug. add back to ensure filenames use valid characters.
            'returnKey = coreEncodingBase64Class.UTF8ToBase64(returnKey)
            Return result
        End Function
        '
        '=======================================================================
        Private Sub appendCacheLog(line As String)
            Try
                logController.appendLog(cpCore, line, "cache")
            Catch ex As Exception
                cpCore.handleException(New ApplicationException("appendCacheLog exception", ex))
            End Try
        End Sub
        '
        '====================================================================================================
        Public Function getString(key As String) As String
            Return genericController.encodeText(getObject(Of String)(key))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' create a cache key for an entity model (based on the Db)
        ''' </summary>
        ''' <param name="tableName"></param>
        ''' <param name="fieldName"></param>
        ''' <param name="fieldValue"></param>
        ''' <returns></returns>
        Public Shared Function getCacheKey_Entity(tableName As String, fieldName As String, fieldValue As String) As String
            Return (tableName & "/" & fieldName & "/" & fieldValue).ToLower().Replace(" ", "_")
        End Function
        Public Shared Function getCacheKey_Entity(tableName As String, recordId As Integer) As String
            Return (tableName & "/id/" & recordId.ToString()).ToLower().Replace(" ", "_")
        End Function
        '
        Public Shared Function getCacheKey_Entity(tableName As String, field1Name As String, field1Value As Integer, field2Name As String, field2Value As Integer) As String
            Return (tableName & "/" & field1Name & "/" & field1Value & "/" & field2Name & "/" & field2Value).ToLower().Replace(" ", "_")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' create a cache name for an object composed of data not from a signel record
        ''' </summary>
        ''' <param name="objectName"></param>
        ''' <param name="uniqueObjectIdentifier"></param>
        ''' <returns></returns>
        Public Shared Function getCacheKey_ComplexObject(objectName As String, uniqueObjectIdentifier As String) As String
            Return ("complexobject/" & objectName & "/" & uniqueObjectIdentifier).ToLower().Replace(" ", "_")
        End Function
        '
        Public Sub invalidateContent_Entity(cpcore As coreClass, tableName As String, recordId As Integer)
            '
            invalidateContent(getCacheKey_Entity(tableName, recordId))
            Select Case tableName.ToLower
                Case Models.Entity.linkAliasModel.contentTableName.ToLower
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
                Case Models.Entity.linkForwardModel.contentTableName.ToLower
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
                Case Models.Entity.addonModel.contentTableName.ToLower
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
            End Select
        End Sub
        '
        '====================================================================================================
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
                    If (cacheClient IsNot Nothing) Then
                        cacheClient.Dispose()
                    End If
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