
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
    ''' Interface to cache systems
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
        Private remoteCacheDisabled As Boolean
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
                    Dim dataObject As cacheObjectClass = getCacheObject("globalInvalidationDate")
                    If (dataObject IsNot Nothing) Then
                        _globalInvalidationDate = dataObject.saveDate
                    End If
                    If (_globalInvalidationDate Is Nothing) Then
                        _globalInvalidationDate = Now()
                        setCacheObject("globalInvalidationDate", New cacheObjectClass With {.saveDate = CDate(_globalInvalidationDate)})
                    End If
                End If
                Return CDate(_globalInvalidationDate)
            End Get
        End Property
        Private _globalInvalidationDate As Date?
        '
        '====================================================================================================
        ''' <summary>
        ''' cache data wrapper to include tags and save datetime
        ''' </summary>
        <Serializable()>
        Public Class cacheObjectClass
            Public primaryObjectKey As String                       ' if populated, all other properties are ignored and the primary tag b
            Public dependantObjectList As New List(Of String)       ' this object is invalidated if any of these objects are invalidated
            Public saveDate As Date                                 ' the date this object was last saved.
            Public invalidationDate As Date                         ' the future date when this object self-invalidates
            Public data As Object                                   ' the data storage
        End Class
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
                remoteCacheDisabled = True
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
                    remoteCacheDisabled = False
                End If
            Catch ex As Exception
                '
                ' client does not throw its own errors, so try to differentiate by message
                '
                cpCore.handleExceptionAndRethrow(New ApplicationException("Exception initializing remote cache, will continue with cache disabled.", ex))
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
        Private Sub setCacheObject(ByVal cacheName As String, ByVal data As cacheObjectClass)
            Try
                If (String.IsNullOrEmpty(cacheName)) Then
                    Throw New ArgumentException("cacheName cannot be blank")
                Else
                    If (cpCore.serverConfig.appConfig.enableCache) And (cpCore.siteProperties.allowCache_notCached) Then
                        Dim encodedCacheName As String = encodeCacheName(cpCore.serverConfig.appConfig.name, cacheName)
                        If cpCore.serverConfig.isLocalCache Or remoteCacheDisabled Then
                            '
                            ' -- implement a simple local cache using the filesystem+dotnet
                            setCacheObjectDotNet(cacheName, data)
                            Dim serializedData As String = Newtonsoft.Json.JsonConvert.SerializeObject(data)
                            Using mutex As New System.Threading.Mutex(False, encodedCacheName)
                                mutex.WaitOne()
                                cpCore.privateFiles.saveFile("appCache\" & genericController.encodeFilename(encodedCacheName & ".txt"), serializedData)
                                mutex.ReleaseMutex()
                            End Using
                        Else
                            '
                            ' -- use remote cache
                            Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, encodedCacheName, data, data.invalidationDate)
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
        ''' dotnet set cache object
        ''' </summary>
        ''' <param name="cacheName"></param>
        ''' <param name="data"></param>
        Public Sub setCacheObjectDotNet(ByVal cacheName As String, ByVal data As cacheObjectClass)
            Dim cache As ObjectCache = MemoryCache.Default
            Dim policy As New CacheItemPolicy()
            policy.AbsoluteExpiration = Now.AddMinutes(100)
            cache.Set(cacheName, data, policy)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dotnet get cache object
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="cacheItemName"></param>
        ''' <param name="cachetimeinminutes"></param>
        ''' <param name="objectSettingFunction"></param>
        ''' <returns></returns>
        Public Function getCacheObjectDotNet(ByVal cacheName As String) As cacheObjectClass
            Dim cache As ObjectCache = MemoryCache.Default
            Dim cachedObject As cacheObjectClass = DirectCast(cache(cacheName), cacheObjectClass)
            Return cachedObject
        End Function
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
                Dim cacheObject As cacheObjectClass
                Dim cacheMiss As Boolean = False
                Dim cacheMissReason As String = ""
                Dim dateCompare As Integer
                '
                If Not (String.IsNullOrEmpty(cacheName)) Then
                    '
                    cacheObject = getCacheObject(cacheName)
                    '
                    If (cacheObject IsNot Nothing) Then
                        dateCompare = globalInvalidationDate.CompareTo(cacheObject.saveDate)
                        If (dateCompare >= 0) Then
                            cacheMissReason = "GetObject(" & cacheName & "), invalidated because the cacheObject's saveDate [" & cacheObject.saveDate.ToString() & "] is after the globalInvalidationDate [" & globalInvalidationDate & "]"
                            appendCacheLog(cacheMissReason)
                        Else
                            '
                            ' if this data is newer that the last global invalidation, continue
                            '
                            If cacheObject.dependantObjectList.Count = 0 Then
                                appendCacheLog("GetObject(" & cacheName & "), no taglist found")
                            Else
                                For Each dependantObjectCacheName As String In cacheObject.dependantObjectList
                                    Dim dependantObject As cacheObjectClass = getCacheObject(dependantObjectCacheName)
                                    If (dependantObject IsNot Nothing) Then
                                        dateCompare = dependantObject.saveDate.CompareTo(cacheObject.saveDate)
                                        'Dim ticks As Long = ((tagInvalidationDate - cacheObject.saveDate).Ticks)
                                        'appendCacheLog("GetObject(" & cacheName & "), tagInvalidationDate[" & tagInvalidationDate & "], cacheData.saveDate[" & cacheObject.saveDate & "], dateCompare[" & dateCompare & "], tick diff [" & ticks & "]")
                                        If (dateCompare >= 0) Then
                                            cacheMiss = True
                                            cacheMissReason = "GetObject(" & cacheName & "), invalidated because the dependantobject [" & dependantObjectCacheName & "] has a saveDate [" & dependantObject.saveDate.ToString() & "] after the cacheObject's dateDate [" & cacheObject.saveDate.ToString() & "]"
                                            appendCacheLog(cacheMissReason)
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                            If Not cacheMiss Then
                                appendCacheLog("GetObject(" & cacheName & "), valid")
                                If (TypeOf cacheObject.data Is Newtonsoft.Json.Linq.JObject) Then
                                    Dim data As Newtonsoft.Json.Linq.JObject = DirectCast(cacheObject.data, Newtonsoft.Json.Linq.JObject)
                                    returnObject = data.ToObject(Of objectClass)()
                                ElseIf (TypeOf cacheObject.data Is Newtonsoft.Json.Linq.JArray) Then
                                    Dim data As Newtonsoft.Json.Linq.JArray = DirectCast(cacheObject.data, Newtonsoft.Json.Linq.JArray)
                                    returnObject = data.ToObject(Of objectClass)()
                                Else
                                    returnObject = DirectCast(cacheObject.data, objectClass)
                                End If
                                'Try

                                '    TryCast(cacheObject.data, Newtonsoft.Json.JsonToken.JObject)

                                '    cacheObject.data.toObject


                                '    returnObject = DirectCast(cacheObject.data, objectClass)
                                '    'returnObject = TryCast(cacheObject.data, objectClass)
                                'Catch ex As Exception
                                '    cacheMissReason = "GetObject(" & cacheName & "), invalidated because the cacheObject.data type [" & cacheObject.data.GetType().ToString() & "] does not match the required return type. Over-writing the cache object."
                                '    appendCacheLog(cacheMissReason)
                                '    setCacheObject(cacheName, New cacheObjectClass With {.data = Nothing})
                                'End Try
                                'If TypeOf cacheObject.data Is objectClass Then
                                '    returnObject = DirectCast(cacheObject.data, objectClass)
                                'Else
                                '    cacheMissReason = "GetObject(" & cacheName & "), invalidated because the cacheObject.data type [" & cacheObject.data.GetType().ToString() & "] does not match the required return type."
                                '    appendCacheLog(cacheMissReason)
                                'End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
        Private Function getCacheObject(ByVal cacheName As String) As cacheObjectClass
            Dim returnObj As cacheObjectClass = Nothing
            Try
                appendCacheLog("getCacheObject, enter [" & cacheName & "]")
                If (String.IsNullOrEmpty(cacheName)) Then
                    Throw New ArgumentException("CacheName cannot be blank")
                Else
                    If (cpCore.serverConfig.appConfig.enableCache) And (cpCore.siteProperties.allowCache_notCached) Then
                        appendCacheLog("getCacheObject, config.enableCache and siteproperty.allowCache")
                        Dim encodedCacheName As String = encodeCacheName(cpCore.serverConfig.appConfig.name, cacheName)
                        If cpCore.serverConfig.isLocalCache Or remoteCacheDisabled Then
                            appendCacheLog("getCacheObject, local cache")
                            '
                            ' implement a simple local cache using the filesystem/dotnet
                            '
                            returnObj = getCacheObjectDotNet(cacheName)
                            If (returnObj) Is Nothing Then
                                appendCacheLog("getCacheObject, memory cache miss")
                                Dim serializedDataObject As String = Nothing
                                Using mutex As New System.Threading.Mutex(False, encodedCacheName)
                                    mutex.WaitOne()
                                    serializedDataObject = cpCore.privateFiles.readFile("appCache\" & genericController.encodeFilename(encodedCacheName & ".txt"))
                                    mutex.ReleaseMutex()
                                End Using
                                If String.IsNullOrEmpty(serializedDataObject) Then
                                    appendCacheLog("getCacheObject, file cache miss")
                                Else
                                    appendCacheLog("getCacheObject, file cache hit, write to memory cache")
                                    returnObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of cacheObjectClass)(serializedDataObject)
                                    setCacheObjectDotNet(cacheName, returnObj)
                                End If
                            End If
                        Else
                            '
                            ' -- use remote cache
                            appendCacheLog("getCacheObject, remote cache")
                            Try
                                returnObj = cacheClient.Get(Of cacheObjectClass)(encodedCacheName)
                            Catch ex As Exception
                                '
                                ' client does not throw its own errors, so try to differentiate by message
                                '
                                If (ex.Message.ToLower.IndexOf("unable to load type") >= 0) Then
                                    '
                                    ' trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                    '
                                    cacheClient.Remove(encodedCacheName)
                                    returnObj = Nothing
                                Else
                                    '
                                    ' some other error
                                    '
                                    cpCore.handleExceptionAndRethrow(ex)
                                End If
                            End Try
                        End If
                        If (returnObj IsNot Nothing) Then
                            '
                            ' -- empty objects return nothing, empty lists return count=0
                            If (returnObj.dependantObjectList Is Nothing) Then
                                returnObj.dependantObjectList = New List(Of String)
                            End If
                        End If
                    End If
                End If
                appendCacheLog("getCacheObject, exit ")
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                If allowCache Then
                    Dim cacheObject As New cacheObjectClass With {
                        .data = data,
                        .saveDate = DateTime.Now(),
                        .invalidationDate = Now.AddDays(invalidationDaysDefault),
                        .dependantObjectList = Nothing
                    }
                    setCacheObject(cacheName, cacheObject)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                If allowCache Then
                    Dim cacheObject As New cacheObjectClass With {
                        .data = data,
                        .saveDate = DateTime.Now(),
                        .invalidationDate = invalidationDate,
                        .dependantObjectList = dependantObjectList
                    }
                    setCacheObject(cacheName, cacheObject)
                End If
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
                If allowCache Then
                    Dim dependantObjectList As New List(Of String)
                    dependantObjectList.Add(dependantObject)
                    Dim cacheObject As New cacheObjectClass With {
                        .data = data,
                        .saveDate = DateTime.Now(),
                        .invalidationDate = invalidationDate,
                        .dependantObjectList = dependantObjectList
                    }
                    setCacheObject(cacheName, cacheObject)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                If allowCache Then
                    Dim cacheObject As New cacheObjectClass With {
                        .data = data,
                        .saveDate = DateTime.Now(),
                        .invalidationDate = Now.AddDays(invalidationDaysDefault),
                        .dependantObjectList = dependantObjectList
                    }
                    setCacheObject(cacheName, cacheObject)
                End If
            Catch ex As Exception
                Try
                    cpCore.handleExceptionAndRethrow(ex)
                Catch errObj As Exception
                End Try
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
                If allowCache Then
                    Dim dependantObjectList As New List(Of String)
                    dependantObjectList.Add(dependantObject)
                    Dim cacheObject As New cacheObjectClass With {
                        .data = data,
                        .saveDate = DateTime.Now(),
                        .invalidationDate = Now.AddDays(invalidationDaysDefault),
                        .dependantObjectList = dependantObjectList
                    }
                    setCacheObject(cacheName, cacheObject)
                End If
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
                Call setCacheObject("globalInvalidationDate", New cacheObjectClass With {.saveDate = Now()})
                _globalInvalidationDate = Nothing
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                If cpCore.serverConfig.appConfig.enableCache Then
                    If Not String.IsNullOrEmpty(cacheName) Then
                        '
                        ' set the object's invalidation date
                        '
                        Dim invalidatedCacheObject As New cacheObjectClass With {
                            .saveDate = Now()
                        }
                        setCacheObject(cacheName, invalidatedCacheObject)
                        '
                        ' test if this tag is a content name
                        '
                        Dim cdef As coreMetaDataClass.CDefClass = cpCore.metaData.getCdef(cacheName)
                        If Not cdef Is Nothing Then
                            '
                            ' Invalidate all child cdef
                            '
                            If cdef.childIdList.Count > 0 Then
                                For Each childId As Integer In cdef.childIdList
                                    Dim childCdef As coreMetaDataClass.CDefClass
                                    childCdef = cpCore.metaData.getCdef(childId)
                                    If Not childCdef Is Nothing Then
                                        setCacheObject(childCdef.Name, invalidatedCacheObject)
                                    End If
                                Next
                            End If
                            '
                            ' Now go up to the top-most parent
                            '
                            Do While cdef.parentID > 0
                                cdef = cpCore.metaData.getCdef(cdef.parentID)
                                If Not cdef Is Nothing Then
                                    setCacheObject(cdef.Name, invalidatedCacheObject)
                                End If
                            Loop
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Clears all baked content in any content definition specified
        '========================================================================
        '
        Public Sub invalidateObjectList(ByVal cacheNameCommaList As String)
            Try
                Dim tagList As New List(Of String)
                '
                If Not String.IsNullOrEmpty(cacheNameCommaList.Trim) Then
                    tagList.AddRange(cacheNameCommaList.Split(","c))
                    invalidateObjectList(tagList)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                If allowCache Then
                    For Each tag In cacheNameList
                        Call invalidateObject(tag)
                    Next
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
        Private Function encodeCacheName(appName As String, sourceKey As String) As String
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
                logController.log_appendLog(cpCore, line, "cache")
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(New ApplicationException("appendCacheLog exception", ex))
            End Try
        End Sub
        '
        Private ReadOnly Property allowCache As Boolean
            Get
                Return cpCore.serverConfig.appConfig.enableCache
            End Get
        End Property
        '
        '====================================================================================================
        Public Function getString(cacheName As String) As String
            Return genericController.encodeText(getObject(Of String)(cacheName))
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