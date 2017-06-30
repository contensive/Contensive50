
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
    ''' When a record is saved in admin, an invalidation call is made for tableName+1d.
    ''' 
    ''' If code edits a db record, you should call invalidation for tablename+id+#
    ''' 
    ''' one-to-many lists: if the list is cached, it has to include tags for all its included members
    ''' 
    ''' many-to-many lists: should have a model for the rule table, and lists should come from methods there. the delete method must invalidate the rule model cache
    ''' 
    ''' A cacheobject has:
    '''   cacheName - if the object is a model of a database record, and there is only one model for that db record, use tableName+id as the cacheName. If it contains other data,
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
        'Private remoteCacheDisabled As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' cache data wrapper to include tags and save datetime
        ''' </summary>
        <Serializable()>
        Public Class cacheWrapperClass
            Public primaryObjectKey As String                       ' if populated, all other properties are ignored and the primary tag b
            Public dependantObjectList As New List(Of String)       ' this object is invalidated if any of these objects are invalidated
            Public saveDate As Date                                 ' the date this object was last saved.
            Public invalidationDate As Date                         ' the future date when this object self-invalidates
            Public data As Object                                   ' the data storage
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property globalInvalidationDate As Date
            Get
                '
                If (_globalInvalidationDate Is Nothing) Then
                    Dim dataObject As cacheWrapperClass = getCacheWrapper("globalInvalidationDate")
                    If (dataObject IsNot Nothing) Then
                        _globalInvalidationDate = dataObject.saveDate
                    End If
                    If (_globalInvalidationDate Is Nothing) Then
                        _globalInvalidationDate = Now()
                        setCacheWrapper("globalInvalidationDate", New cacheWrapperClass With {.saveDate = CDate(_globalInvalidationDate)})
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
                throw (New ApplicationException("Exception initializing remote cache, will continue with cache disabled.", ex))
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save object directly to cache.
        ''' </summary>
        ''' <param name="cacheName"></param>
        ''' <param name="data">Either a string, a date, or a serializable object</param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub setCacheWrapper(ByVal cacheName As String, ByVal data As cacheWrapperClass)
            Try
                If (String.IsNullOrEmpty(cacheName)) Then
                    Throw New ArgumentException("cacheName cannot be blank")
                Else
                    Dim cacheWrapperName As String = encodeCacheWrapperName(cpCore.serverConfig.appConfig.name, cacheName)
                    If (cpCore.serverConfig.enableLocalMemoryCache) Then
                        If (remoteCacheInitialized) Then
                            '
                            ' -- save remote cache
                            Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheWrapperName, data, data.invalidationDate)
                        End If
                        If cpCore.serverConfig.enableLocalMemoryCache Then
                            '
                            ' -- save local memory cache
                            setCacheWrapper_MemoryCache(cacheWrapperName, data)
                        End If
                        If cpCore.serverConfig.enableLocalFileCache Then
                            '
                            ' -- save local memory cache
                            Dim serializedData As String = Newtonsoft.Json.JsonConvert.SerializeObject(data)
                            Using mutex As New System.Threading.Mutex(False, cacheWrapperName)
                                mutex.WaitOne()
                                cpCore.privateFiles.saveFile("appCache\" & genericController.encodeFilename(cacheWrapperName & ".txt"), serializedData)
                                mutex.ReleaseMutex()
                            End Using
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save cacheWrapper to memory cache
        ''' </summary>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        Public Sub setCacheWrapper_MemoryCache(ByVal cacheName As String, ByVal data As cacheWrapperClass)
            Dim cache As ObjectCache = MemoryCache.Default
            Dim policy As New CacheItemPolicy()
            policy.AbsoluteExpiration = Now.AddMinutes(100)
            cache.Set(cacheName, data, policy)
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' dotnet get cache object
        '''' </summary>
        '''' <typeparam name="T"></typeparam>
        '''' <param name="cacheItemName"></param>
        '''' <param name="cachetimeinminutes"></param>
        '''' <param name="objectSettingFunction"></param>
        '''' <returns></returns>
        'Public Function getCacheWrapperDotNet(ByVal cacheName As String) As cacheObjectClass
        '    Dim cache As ObjectCache = MemoryCache.Default
        '    Dim cachedObject As cacheObjectClass = DirectCast(cache(cacheName), cacheObjectClass)
        '    Return cachedObject
        'End Function
        '/// <summary>
        '/// A generic method for getting And setting objects to the memory cache.
        '/// </summary>
        '/// <typeparam name="T">The type of the object to be returned.</typeparam>
        '/// <param name="cacheItemName">The name to be used when storing this object in the cache.</param>
        '/// <param name="cacheTimeInMinutes">How long to cache this object for.</param>
        '/// <param name="objectSettingFunction">A parameterless function to call if the object isn't in the cache and you need to set it.</param>
        '/// <returns>An object of the type you asked for</returns>
        'Public Static T GetObjectFromCache<T>(String cacheItemName, int cacheTimeInMinutes, Func<T> objectSettingFunction)
        '{
        '    ObjectCache cache = MemoryCache.Default;
        '    var cachedObject = (T)cache[cacheItemName];
        '    If (cachedObject == null)
        '    {
        '        CacheItemPolicy policy = New CacheItemPolicy();
        '        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTimeInMinutes);
        '        cachedObject = objectSettingFunction();
        '        cache.Set(cacheItemName, cachedObject, policy);
        '    }
        '    Return cachedObject;
        '}
        '
        '========================================================================
        ''' <summary>
        ''' get an object from cache. If the cache misses or is invalidated, nothing object is returned
        ''' </summary>
        ''' <typeparam name="objectClass"></typeparam>
        ''' <param name="cacheName"></param>
        ''' <returns></returns>
        Public Function getObject(Of objectClass)(ByVal cacheName As String) As objectClass
            Dim returnObject As objectClass = Nothing
            Try
                Dim cacheWrapper As cacheWrapperClass
                Dim cacheMiss As Boolean = False
                Dim dateCompare As Integer
                '
                If Not (String.IsNullOrEmpty(cacheName)) Then
                    '
                    ' -- read cacheWrapper
                    appendCacheLog("getObject(" & cacheName & "), enter") : Dim sw As Stopwatch = Stopwatch.StartNew()
                    cacheWrapper = getCacheWrapper(cacheName)
                    If (cacheWrapper IsNot Nothing) Then
                        '
                        ' -- test for global invalidation
                        dateCompare = globalInvalidationDate.CompareTo(cacheWrapper.saveDate)
                        If (dateCompare >= 0) Then
                            '
                            ' -- global invalidation
                            appendCacheLog("GetObject(" & cacheName & "), invalidated because the cacheObject's saveDate [" & cacheWrapper.saveDate.ToString() & "] is after the globalInvalidationDate [" & globalInvalidationDate & "]")
                        Else
                            '
                            ' -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
                            If cacheWrapper.dependantObjectList.Count = 0 Then
                                '
                                ' -- no dependent objects
                                appendCacheLog("GetObject(" & cacheName & "), no dependantObjectList")
                            Else
                                For Each dependantObjectCacheName As String In cacheWrapper.dependantObjectList
                                    Dim dependantObject As cacheWrapperClass = getCacheWrapper(dependantObjectCacheName)
                                    If (dependantObject IsNot Nothing) Then
                                        dateCompare = dependantObject.saveDate.CompareTo(cacheWrapper.saveDate)
                                        If (dateCompare >= 0) Then
                                            cacheMiss = True
                                            appendCacheLog("GetObject(" & cacheName & "), invalidated because the dependantobject [" & dependantObjectCacheName & "] has a saveDate [" & dependantObject.saveDate.ToString() & "] after the cacheObject's dateDate [" & cacheWrapper.saveDate.ToString() & "]")
                                            Exit For
                                        End If
                                    End If
                                Next
                                If Not cacheMiss Then
                                    If (TypeOf cacheWrapper.data Is Newtonsoft.Json.Linq.JObject) Then
                                        Dim data As Newtonsoft.Json.Linq.JObject = DirectCast(cacheWrapper.data, Newtonsoft.Json.Linq.JObject)
                                        returnObject = data.ToObject(Of objectClass)()
                                    ElseIf (TypeOf cacheWrapper.data Is Newtonsoft.Json.Linq.JArray) Then
                                        Dim data As Newtonsoft.Json.Linq.JArray = DirectCast(cacheWrapper.data, Newtonsoft.Json.Linq.JArray)
                                        returnObject = data.ToObject(Of objectClass)()
                                    Else
                                        returnObject = DirectCast(cacheWrapper.data, objectClass)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    appendCacheLog("getObject(" & cacheName & "), exit(" & sw.ElapsedMilliseconds & "ms)") : sw.Stop()
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnObject
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a cache object from the cache. returns the cacheObject that wraps the object
        ''' </summary>
        ''' <typeparam name="returnType"></typeparam>
        ''' <param name="cacheName"></param>
        ''' <returns></returns>
        Private Function getCacheWrapper(ByVal cacheName As String) As cacheWrapperClass
            Dim returnObj As cacheWrapperClass = Nothing
            Try
                appendCacheLog("getCacheWrapper(" & cacheName & "), enter ")
                If (String.IsNullOrEmpty(cacheName)) Then
                    Throw New ArgumentException("CacheName cannot be blank")
                Else
                    Dim cacheWrapperName As String = encodeCacheWrapperName(cpCore.serverConfig.appConfig.name, cacheName)
                    If (remoteCacheInitialized) Then
                        '
                        ' -- use remote cache
                        Try
                            returnObj = cacheClient.Get(Of cacheWrapperClass)(cacheWrapperName)
                        Catch ex As Exception
                            '
                            ' --client does not throw its own errors, so try to differentiate by message
                            If (ex.Message.ToLower.IndexOf("unable to load type") >= 0) Then
                                '
                                ' -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                cacheClient.Remove(cacheWrapperName)
                                returnObj = Nothing
                            Else
                                '
                                ' -- some other error
                                cpCore.handleExceptionAndContinue(ex) : Throw
                            End If
                        End Try
                        If (returnObj IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & cacheName & "), remoteCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & cacheName & "), remoteCache miss")
                        End If
                    End If
                    If (returnObj Is Nothing) And cpCore.serverConfig.enableLocalMemoryCache Then
                        '
                        ' -- local memory cache
                        'Dim cache As ObjectCache = MemoryCache.Default
                        returnObj = DirectCast(MemoryCache.Default(cacheWrapperName), cacheWrapperClass)
                        If (returnObj IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & cacheName & "), memoryCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & cacheName & "), memoryCache miss")
                        End If
                    End If
                    If (returnObj Is Nothing) And cpCore.serverConfig.enableLocalFileCache Then
                        '
                        ' -- local file cache
                        Dim serializedDataObject As String = Nothing
                        Using mutex As New System.Threading.Mutex(False, cacheWrapperName)
                            mutex.WaitOne()
                            serializedDataObject = cpCore.privateFiles.readFile("appCache\" & genericController.encodeFilename(cacheWrapperName & ".txt"))
                            mutex.ReleaseMutex()
                        End Using
                        If String.IsNullOrEmpty(serializedDataObject) Then
                            appendCacheLog("getCacheWrapper(" & cacheName & "), file miss")
                        Else
                            appendCacheLog("getCacheWrapper(" & cacheName & "), file hit, write to memory cache")
                            returnObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of cacheWrapperClass)(serializedDataObject)
                            setCacheWrapper_MemoryCache(cacheWrapperName, returnObj)
                        End If
                        If (returnObj IsNot Nothing) Then
                            appendCacheLog("getCacheWrapper(" & cacheName & "), fileCache hit")
                        Else
                            appendCacheLog("getCacheWrapper(" & cacheName & "), fileCache miss")
                        End If
                    End If

                    'If (cpCore.serverConfig.appConfig.enableCache) And (cpCore.siteProperties.allowCache_notCached) Then
                    '    If cpCore.serverConfig.enableLocalMemoryCache Or remoteCacheDisabled Then
                    '        '
                    '        ' -- implement a simple local cache using the filesystem/dotnet
                    '        'Dim cache As ObjectCache = MemoryCache.Default
                    '        returnObj = DirectCast(MemoryCache.Default(cacheName), cacheWrapperClass)
                    '        'returnObj = getCacheWrapperDotNet(cacheName)
                    '        If (returnObj IsNot Nothing) Then
                    '            appendCacheLog("getCacheWrapper(" & cacheName & "), memory hit")
                    '        Else
                    '            appendCacheLog("getCacheWrapper(" & cacheName & "), memory miss")
                    '            Dim serializedDataObject As String = Nothing
                    '            Using mutex As New System.Threading.Mutex(False, encodedCacheName)
                    '                mutex.WaitOne()
                    '                serializedDataObject = cpCore.privateFiles.readFile("appCache\" & genericController.encodeFilename(encodedCacheName & ".txt"))
                    '                mutex.ReleaseMutex()
                    '            End Using
                    '            If String.IsNullOrEmpty(serializedDataObject) Then
                    '                appendCacheLog("getCacheWrapper(" & cacheName & "), file miss")
                    '            Else
                    '                appendCacheLog("getCacheWrapper(" & cacheName & "), file hit, write to memory cache")
                    '                returnObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of cacheWrapperClass)(serializedDataObject)
                    '                setCacheWrapper_MemoryCache(cacheName, returnObj)
                    '            End If
                    '        End If
                    '    Else
                    '        ''
                    '        '' -- use remote cache
                    '        'Try
                    '        '    returnObj = cacheClient.Get(Of cacheWrapperClass)(encodedCacheName)
                    '        'Catch ex As Exception
                    '        '    '
                    '        '    ' --client does not throw its own errors, so try to differentiate by message
                    '        '    If (ex.Message.ToLower.IndexOf("unable to load type") >= 0) Then
                    '        '        '
                    '        '        ' -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                    '        '        cacheClient.Remove(encodedCacheName)
                    '        '        returnObj = Nothing
                    '        '    Else
                    '        '        '
                    '        '        ' -- some other error
                    '        '        cpCore.handleExceptionAndContinue(ex) : Throw
                    '        '    End If
                    '        'End Try
                    '    End If
                    'End If
                    If (returnObj IsNot Nothing) Then
                        '
                        ' -- empty objects return nothing, empty lists return count=0
                        If (returnObj.dependantObjectList Is Nothing) Then
                            returnObj.dependantObjectList = New List(Of String)
                        End If
                    End If
                End If
                appendCacheLog("getCacheWrapper(" & cacheName & "), exit ")
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnObj
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save a string to cache, for compatibility with existing site. (no objects, no invalidation, yet)
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="cacheObject"></param>
        ''' <remarks></remarks>
        Public Sub setObject(cacheName As String, data As Object)
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .data = data,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .dependantObjectList = Nothing
                }
                setCacheWrapper(cacheName, cacheWrapper)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save an object to cache, with invalidation
        ''' 
        ''' </summary>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="dependantObjectList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setObject(cacheName As String, data As Object, invalidationDate As Date, dependantObjectList As List(Of String))
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .data = data,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = invalidationDate,
                    .dependantObjectList = dependantObjectList
                }
                setCacheWrapper(cacheName, cacheWrapper)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save an object to cache, with invalidation
        ''' 
        ''' </summary>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="dependantObject">The cache name of an object that .</param>
        ''' <remarks></remarks>
        Public Sub setObject(cacheName As String, data As Object, invalidationDate As Date, dependantObject As String)
            Try
                Dim dependantObjectList As New List(Of String)
                dependantObjectList.Add(dependantObject)
                Dim cacheWrapper As New cacheWrapperClass With {
                    .data = data,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = invalidationDate,
                    .dependantObjectList = dependantObjectList
                }
                setCacheWrapper(cacheName, cacheWrapper)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value, compatible with legacy method signature.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        ''' <param name="dependantObjectList">List of tags. Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setObject(cacheName As String, data As Object, dependantObjectList As List(Of String))
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .data = data,
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .dependantObjectList = dependantObjectList
                }
                setCacheWrapper(cacheName, cacheWrapper)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value, compatible with legacy method signature.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        ''' <param name="dependantObject"></param>
        ''' <remarks></remarks>
        Public Sub setObject(cacheName As String, data As Object, dependantObject As String)
            Try
                'Dim dependantObjectList As New List(Of String)
                'dependantObjectList.Add(dependantObject)
                setObject(cacheName, data, New List(Of String)({dependantObject}))
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a secondary cache entry. Contains no data, just a reference to the primary cache where the data is stored
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        ''' <param name="dependantObject"></param>
        ''' <remarks></remarks>
        Public Sub setSecondaryObject(cacheName As String, primaryCacheName As String)
            Try
                Dim cacheWrapper As New cacheWrapperClass With {
                    .saveDate = DateTime.Now(),
                    .invalidationDate = Now.AddDays(invalidationDaysDefault),
                    .primaryObjectKey = primaryCacheName
                }
                setCacheWrapper(cacheName, cacheWrapper)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
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
                Call setCacheWrapper("globalInvalidationDate", New cacheWrapperClass With {.saveDate = Now()})
                _globalInvalidationDate = Nothing
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' invalidates a tag
        '''' </summary>
        '''' <param name="tag"></param>
        '''' <remarks></remarks>
        Public Sub invalidateObject(ByVal cacheName As String)
            Try
                If Not String.IsNullOrEmpty(cacheName.Trim()) Then
                    setCacheWrapper(cacheName, New cacheWrapperClass With {.saveDate = Now()})
                    ''
                    '' set the object's invalidation date
                    ''
                    'Dim invalidatedCacheWrapper As New cacheWrapperClass With {
                    '        .saveDate = Now()
                    '    }
                    'setCacheWrapper(cacheName, New cacheWrapperClass With {.saveDate = Now()})
                    '
                    ' -- this is expensive, cache objects should be tables, and parent cdef was always a bad idea
                    ''
                    '' test if this tag is a content name
                    ''
                    'Dim cdef As coreMetaDataClass.CDefClass = cpCore.metaData.getCdef(cacheName)
                    'If Not cdef Is Nothing Then
                    '    '
                    '    ' Invalidate all child cdef
                    '    '
                    '    If cdef.childIdList.Count > 0 Then
                    '        For Each childId As Integer In cdef.childIdList
                    '            Dim childCdef As coreMetaDataClass.CDefClass
                    '            childCdef = cpCore.metaData.getCdef(childId)
                    '            If Not childCdef Is Nothing Then
                    '                setCacheWrapper(childCdef.Name, invalidatedCacheWrapper)
                    '            End If
                    '        Next
                    '    End If
                    '    '
                    '    ' Now go up to the top-most parent
                    '    '
                    '    Do While cdef.parentID > 0
                    '        cdef = cpCore.metaData.getCdef(cdef.parentID)
                    '        If Not cdef Is Nothing Then
                    '            setCacheWrapper(cdef.Name, invalidatedCacheWrapper)
                    '        End If
                    '    Loop
                    'End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' update the dbTablename cache entry. Do this anytime any record is updated in the table
        ''' </summary>
        ''' <param name="ContentName"></param>
        Public Sub invalidateContent(ByVal ContentName As String)
            Try
                invalidateDbTable(cpCore.metaData.getContentTablename(ContentName))
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' invalidate all cache entries to this table. "dbTableName"
        ''' when any cache is saved, it should include a dependancy on a cachename=dbtablename
        ''' </summary>
        ''' <param name="dbTableName"></param>
        Public Sub invalidateDbTable(ByVal dbTableName As String)
            Try
                invalidateObject(dbTableName.ToLower().Replace(" ", "_"))
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a list of tags 
        ''' </summary>
        ''' <param name="cacheNameList"></param>
        ''' <remarks></remarks>
        Public Sub invalidateObjectList(ByVal cacheNameList As List(Of String))
            Try
                For Each cacheName In cacheNameList
                    Call invalidateObject(cacheName)
                Next
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Private Function encodeCacheWrapperName(appName As String, sourceKey As String) As String
            Dim returnKey As String
            returnKey = appName & "-" & sourceKey
            returnKey = Regex.Replace(returnKey, "0x[a-fA-F\d]{2}", "_")
            returnKey = returnKey.Replace(" ", "_")
            '
            ' -- 20170515 JK tmp - makes it harder to debug. add back to ensure filenames use valid characters.
            'returnKey = coreEncodingBase64Class.UTF8ToBase64(returnKey)
            Return returnKey
        End Function
        '
        '=======================================================================
        Private Sub appendCacheLog(line As String)
            Try
                logController.appendLog(cpCore, line, "cache")
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(New ApplicationException("appendCacheLog exception", ex))
            End Try
        End Sub
        ''
        'Private ReadOnly Property allowCache As Boolean
        '    Get
        '        Return cpCore.serverConfig.appConfig.enableCache
        '    End Get
        'End Property
        '
        '====================================================================================================
        Public Function getString(cacheName As String) As String
            Return genericController.encodeText(getObject(Of String)(cacheName))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' produce a standard format cachename for this model
        ''' </summary>
        ''' <param name="tableName"></param>
        ''' <param name="fieldName"></param>
        ''' <param name="fieldValue"></param>
        ''' <returns></returns>
        Public Shared Function getDbRecordCacheName(tableName As String, fieldName As String, fieldValue As String) As String
            Return (tableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        '
        Public Shared Function getDbRecordCacheName(tableName As String, field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
            Return (tableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        End Function
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