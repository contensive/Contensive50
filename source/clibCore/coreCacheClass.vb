
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' Interface to cache systems
    ''' </summary>
    Public Class coreCacheClass
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        ' ----- objects constructed that must be disposed
        '
        Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- constants
        '
        Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- private globals
        '
        Private rightNow As Date
        Private cacheLogPrefix As String
        Private remoteCacheDisabled As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' cache data wrapper to include tags and save datetime
        ''' </summary>
        <Serializable()>
        Public Class cacheDataClass
            Public tagList As New List(Of String)
            Public saveDate As Date
            Public invalidationDate As Date
            Public data As Object
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' get an object from cache based on a key name (it will be appended with clusterName and appName)
        ''' </summary>
        ''' <typeparam name="returnType"></typeparam>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Private Function getObjectRaw(Of returnType)(ByVal key As String) As returnType
            Dim returnObj As returnType = Nothing
            Dim hint As Integer = 0
            Try
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("Cache key cannot be blank")
                Else
                    hint = 100
                    If (Not cpCore.serverconfig.appConfig.enableCache) Or (Not cpCore.siteProperties.allowCache_notCached) Then
                        returnObj = returnObj
                    Else
                        hint = 200
                        Dim rawCacheName As String = encodeCacheKey(cpCore.serverconfig.appConfig.name, key)
                        If cpCore.serverConfig.isLocalCache Or remoteCacheDisabled Then
                            '
                            ' implement a simple local cache using the filesystem
                            '
                            hint = 300
                            Dim serializedDataObject As String = Nothing
                            Using mutex As New System.Threading.Mutex(False, rawCacheName)
                                mutex.WaitOne()
                                serializedDataObject = cpCore.privateFiles.readFile("appCache\" & encodeFilename(rawCacheName & ".txt"))
                                mutex.ReleaseMutex()
                            End Using
                            If String.IsNullOrEmpty(serializedDataObject) Then
                                returnObj = Nothing
                            Else
                                'returnObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of returnType)(serializedDataObject)
                                returnObj = cpCore.json.Deserialize(Of returnType)(serializedDataObject)
                            End If
                        Else
                            '
                            ' use remote cache
                            '
                            hint = 400
                            Try
                                returnObj = cacheClient.Get(Of returnType)(rawCacheName)
                            Catch ex As Exception
                                '
                                ' client does not throw its own errors, so try to differentiate by message
                                '
                                If (ex.Message.ToLower.IndexOf("unable to load type") >= 0) Then
                                    '
                                    ' trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                    '
                                    cacheClient.Remove(rawCacheName)
                                    returnObj = Nothing
                                Else
                                    '
                                    ' some other error
                                    '
                                    cpCore.handleExceptionAndRethrow(ex)
                                End If
                            End Try
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex, "getObjectRaw(" & key & "), hint:" & hint.ToString())
            End Try
            Return returnObj
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' new constructor. Initializes properties and cache client. For now, called from each public routine. Eventually this is contructor.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            Dim logMsg As String = "new cache instance"
            Try
                Me.cpCore = cpCore
                Dim cacheEndpointSplit As String()
                Dim cacheEndpointPort As Integer
                Dim cacheEndpoint As String
                Dim cacheConfig As Amazon.ElastiCacheCluster.ElastiCacheClusterConfig
                '
                rightNow = DateTime.Now()
                cacheLogPrefix = "cacheLog"
                '
                remoteCacheDisabled = True
                cacheEndpoint = cpCore.serverConfig.awsElastiCacheConfigurationEndpoint
                If String.IsNullOrEmpty(cacheEndpoint) Then
                    '
                    logMsg &= ", elasticache disabled"
                    '
                Else
                    cacheEndpointSplit = cacheEndpoint.Split(":"c)
                    If cacheEndpointSplit.Count > 1 Then
                        cacheEndpointPort = EncodeInteger(cacheEndpointSplit(1))
                    Else
                        cacheEndpointPort = 11211
                    End If
                    cacheConfig = New Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(cacheEndpointSplit(0), cacheEndpointPort)
                    cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary
                    cacheClient = New Enyim.Caching.MemcachedClient(cacheConfig)
                    remoteCacheDisabled = False
                End If
                '
                ' initialize cache - load global cache invalidation date
                '
                appendCacheLog(logMsg)
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
        ''' returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property globalInvalidationDate As Date
            Get
                '
                If Not _globalInvalidationDateLoaded Then
                    _globalInvalidationDateLoaded = True
                    Dim dataObject As Object = getObjectRaw(Of Date)("globalInvalidationDate")
                    If TypeOf (dataObject) Is Date Then
                        _globalInvalidationDate = DirectCast(dataObject, Date)
                    ElseIf TypeOf (dataObject) Is String Then
                        _globalInvalidationDate = EncodeDate(dataObject)
                    Else
                        _globalInvalidationDate = Date.MinValue
                    End If
                End If
                Return _globalInvalidationDate
            End Get
        End Property
        Private _globalInvalidationDateLoaded As Boolean = False
        Private _globalInvalidationDate As Date
        '
        '====================================================================================================
        ''' <summary>
        ''' write object directly to cache, no tests. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="data">Either a string, a date, or a serializable object</param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub setKeyRaw(ByVal Key As String, ByVal data As Object, invalidationDate As Date)
            Try
                If (String.IsNullOrEmpty(Key)) Then
                    Throw New ArgumentException("Cache key cannot be blank")
                Else
                    If (cpCore.serverconfig.appConfig.enableCache) And (cpCore.siteProperties.allowCache_notCached) Then
                        Dim rawCacheName As String = encodeCacheKey(cpCore.serverconfig.appConfig.name, Key)
                        If cpCore.serverConfig.isLocalCache Or remoteCacheDisabled Then
                            '
                            ' implement a simple local cache using the filesystem
                            '
                            'Dim serializedData As String = Newtonsoft.Json.JsonConvert.SerializeObject(data)
                            Dim serializedData As String = cpCore.json.Serialize(data)
                            Using mutex As New System.Threading.Mutex(False, rawCacheName)
                                mutex.WaitOne()
                                cpCore.privateFiles.saveFile("appCache\" & encodeFilename(rawCacheName & ".txt"), serializedData)
                                mutex.ReleaseMutex()
                            End Using
                        Else
                            '
                            ' use remote cache
                            '
                            Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, rawCacheName, data, invalidationDate)
                        End If
                    End If
                End If
                'If (String.IsNullOrEmpty(Key)) Then
                '    Throw New ArgumentException("Cache key cannot be blank")
                'ElseIf (invalidationDate.CompareTo(Now) <= 0) Then
                '    Throw New ArgumentException("Cache invalidation date cannot be in the past")
                'Else
                '    If remoteCacheDisabled Then
                '        Throw New NotImplementedException("Local cache mode is not implemented yet")
                '        'cp.Cache.Save(encodeCacheKey(Key), dataString, , invalidationDate)
                '    Else
                '        Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, encodeCacheKey(cpCore.serverconfig.appConfig.name & "-" & Key), data, invalidationDate)
                '    End If
                'End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' write a cacheData object to cache. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="cacheData"></param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub setKey(ByVal Key As String, ByVal cacheData As cacheDataClass, Optional invalidationDate As Date = #12:00:00 AM#)
            Try
                Dim allowSave As Boolean
                '
                If (invalidationDate = #12:00:00 AM#) Then
                    '
                    ' date option excluded, use default date
                    '
                    invalidationDate = invalidationDateDefault
                ElseIf (invalidationDate > Now()) Then
                    '
                    ' use the provided invalidation date
                    '
                    setKeyRaw(Key, cacheData, invalidationDate)
                Else
                    '
                    ' invalidate this key
                    '
                    setKeyRaw(Key, Nothing, invalidationDateDefault)
                End If

                If (String.IsNullOrEmpty(Key)) Then
                    cpCore.handleExceptionAndRethrow(New Exception("key cannot be empty"))
                ElseIf (invalidationDate <= DateTime.Now()) Then
                    cpCore.handleExceptionAndRethrow(New Exception("invalidationDate must be > current date/time"))
                Else
                    allowSave = False
                    If cacheData Is Nothing Then
                        allowSave = True
                    ElseIf Not cacheData.GetType.IsSerializable Then
                        cpCore.handleExceptionAndRethrow(New Exception("Data object must be serializable"))
                    Else
                        allowSave = True
                    End If
                    If allowSave Then
                        setKeyRaw(Key, cacheData, invalidationDate)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a string to cache, for compatibility with existing site. (no objects, no invalidation, yet)
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="key"></param>
        ''' <param name="cacheObject"></param>
        ''' <remarks></remarks>
        Public Sub setKey(key As String, cacheObject As Object)
            Try
                Call setKey(key, cacheObject, invalidationDateDefault, New List(Of String))
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
        ''' <param name="key"></param>
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="invalidationTagList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setKey(key As String, cacheObject As Object, invalidationDate As Date, invalidationTagList As List(Of String))
            Try
                '    '
                '    If TypeOf cacheObject Is String Then
                '        appendCacheLog(vbTab & "save( key(" & key & "), length(" & DirectCast(cacheObject, String).Length() & ")")
                '    Else
                '        appendCacheLog(vbTab & "save( key(" & key & "), obj")
                '    End If
                '
                Dim cacheData As cacheDataClass
                '
                If allowCache Then
                    'addCacheNameToList(CP, cacheName)
                    cacheData = New cacheDataClass()
                    cacheData.data = cacheObject
                    cacheData.saveDate = DateTime.Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = invalidationTagList
                    '
                    setKeyRaw(key, cacheData, invalidationDate)
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
        ''' <param name="key"></param>
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="invalidationTagList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setKey(key As String, cacheObject As Object, invalidationDate As Date, invalidationTag As String)
            Try
                If allowCache Then
                    Dim invalidationTagList As New List(Of String)
                    Dim cacheData As New cacheDataClass()
                    '
                    invalidationTagList.Add(invalidationTag)
                    cacheData.data = cacheObject
                    cacheData.saveDate = DateTime.Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = invalidationTagList
                    '
                    setKeyRaw(key, cacheData, invalidationDate)
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
        ''' <param name="key"></param>
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationTagList">List of tags. Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setKey(key As String, cacheObject As Object, invalidationTagList As List(Of String))
            Try
                Call setKey(key, cacheObject, invalidationDateDefault, invalidationTagList)
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
        ''' <param name="key"></param>
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationTagList">Comma delimited list of tags. Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub setKey(key As String, cacheObject As Object, invalidationTag As String)
            Try
                Dim invalidationTagList As New List(Of String)
                '
                invalidationTagList.Add(invalidationTag)
                '
                Call setKey(key, cacheObject, invalidationDateDefault, invalidationTagList)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' clear all cache entries with the globalInvalidationDate. Legacy code saved all cacheNames in a List() and iterated through the list, then did a contensive clearAll
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <remarks></remarks>
        Public Sub flushCache(ByVal CP As Contensive.BaseClasses.CPBaseClass)
            Try
                '
                appendCacheLog(vbTab & "flushCache(" & CP.Doc.StartTime & ")")
                '
                invalidateAll()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' return the last dateTime this tag was modified
        ''' </summary>
        ''' <param name="tag">A tag that represents the source of the data in a cache entry. When that data source is modifed, the tag can be used to invalidate the cache entries based on it.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getTagInvalidationDate(ByVal tag As String) As Date
            Dim returnTagInvalidationDate As Date = Date.MinValue
            Try
                Dim cacheNameTagInvalidateDate As String = getTagInvalidationDateCacheName(tag)
                'Dim cacheObject As Date
                '
                If allowCache Then
                    If Not String.IsNullOrEmpty(tag) Then
                        '
                        ' get it from raw cache
                        '
                        returnTagInvalidationDate = getObjectRaw(Of Date)(cacheNameTagInvalidateDate)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnTagInvalidationDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates the entire cache (except those entires written with saveRaw)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub invalidateAll()
            Try
                '
                Call setKeyRaw("globalInvalidationDate", Now(), Now().AddYears(10))
                _globalInvalidationDateLoaded = False
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
        Public Sub invalidateTag(ByVal tag As String)
            Try
                Dim cacheName As String = getTagInvalidationDateCacheName(tag)
                Dim cdef As coreMetaDataClass.CDefClass
                '
                appendCacheLog("invalidateTag(" & tag & "), tagInvalidationDateCacheName [" & cacheName & "]")
                '
                If cpCore.serverconfig.appConfig.enableCache Then
                    If Not String.IsNullOrEmpty(tag) Then
                        '
                        ' set the tags invalidation date
                        '
                        setKeyRaw(cacheName, Now, Now.AddDays(invalidationDaysDefault))
                        '
                        ' test if this tag is a content name
                        '
                        cdef = cpCore.metaData.getCdef(tag)
                        If Not cdef Is Nothing Then
                            '
                            ' Invalidate all child cdef
                            '
                            If cdef.childIdList.Count > 0 Then
                                For Each childId As Integer In cdef.childIdList
                                    Dim childCdef As coreMetaDataClass.CDefClass
                                    childCdef = cpCore.metaData.getCdef(childId)
                                    If Not childCdef Is Nothing Then
                                        cacheName = getTagInvalidationDateCacheName(childCdef.Name)
                                        setKeyRaw(cacheName, Now, Now.AddDays(invalidationDaysDefault))
                                    End If
                                Next
                            End If
                            '
                            ' Now go up to the top-most parent
                            '
                            Do While cdef.parentID > 0
                                cdef = cpCore.metaData.getCdef(cdef.parentID)
                                If Not cdef Is Nothing Then
                                    cacheName = getTagInvalidationDateCacheName(cdef.Name)
                                    setKeyRaw(cacheName, Now, Now.AddDays(invalidationDaysDefault))
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
        Public Sub invalidateTagCommaList(ByVal tagCommaList As String)
            Try
                Dim tagList As New List(Of String)
                '
                If Not String.IsNullOrEmpty(tagCommaList.Trim) Then
                    tagList.AddRange(tagCommaList.Split(","c))
                    invalidateTagList(tagList)
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
        ''' <param name="tagList"></param>
        ''' <remarks></remarks>
        Public Sub invalidateTagList(ByVal tagList As List(Of String))
            Try
                If allowCache Then
                    For Each tag In tagList
                        Call invalidateTag(tag)
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
        Private Function encodeCacheKey(appName As String, sourceKey As String) As String
            Dim returnKey As String
            returnKey = appName & "-" & sourceKey
            returnKey = Regex.Replace(returnKey, "0x[a-fA-F\d]{2}", "_")
            returnKey = returnKey.Replace(" ", "_")
            returnKey = coreEncodingBase64Class.UTF8ToBase64(returnKey)
            Return returnKey
        End Function
        '
        '=======================================================================
        Private Sub appendCacheLog(line As String)
            Try
                'cpCore.appendLog(line)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(New ApplicationException("appendCacheLog exception", ex))
            End Try
        End Sub
        '
        Private ReadOnly Property allowCache As Boolean
            Get
                Return cpCore.serverconfig.appConfig.enableCache
            End Get
        End Property
        '
        '========================================================================
        '   Read a string from Bake Cache
        '
        '   Attempts to read from Cache.
        '   If successful
        '       it gets the DependencyString from the first line if the cachestring
        '           DependencyString = ContentName + ":" + RecordCount + " " + LatestModifiedDate [+ ";" + more]
        '       it then generates a DependencyString from the DependencyString:ContentNames providedh
        '       If they match, the result cachestring is returned, else "" is returned and the cache is cleared
        '========================================================================
        '
        Public Function getObject(Of resultType)(ByVal key As String) As resultType
            Dim returnObject As resultType = Nothing
            Try
                Dim cacheData As cacheDataClass
                Dim invalidate As Boolean = False
                Dim dateCompare As Integer
                '
                'return_cacheHit = False
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("cache.GetObject, key cannot be blank.")
                Else
                    '
                    ' block all cache if editing or rendering edits
                    '
                    cacheData = getObjectRaw(Of cacheDataClass)(key)

                    If Not (cacheData Is Nothing) Then
                        dateCompare = globalInvalidationDate.CompareTo(cacheData.saveDate)
                        If (dateCompare >= 0) Then
                            appendCacheLog("GetObject(" & key & "), invalidated, cache_globalInvalidationDate[" & globalInvalidationDate & "] >= saveDate[" & cacheData.saveDate & "], dateCompare[" & dateCompare & "]")
                        Else
                            '
                            ' if this data is newer that the last global invalidation, continue
                            '
                            Dim tagInvalidationDate As Date
                            If cacheData.tagList.Count = 0 Then
                                appendCacheLog("GetObject(" & key & "), no taglist found")
                            Else
                                For Each tag As String In cacheData.tagList
                                    tagInvalidationDate = getTagInvalidationDate(tag)
                                    dateCompare = tagInvalidationDate.CompareTo(cacheData.saveDate)
                                    Dim ticks As Long = ((tagInvalidationDate - cacheData.saveDate).Ticks)
                                    appendCacheLog("GetObject(" & key & "), tagInvalidationDate[" & tagInvalidationDate & "], cacheData.saveDate[" & cacheData.saveDate & "], dateCompare[" & dateCompare & "], tick diff [" & ticks & "]")
                                    If (dateCompare >= 0) Then
                                        invalidate = True
                                        appendCacheLog("GetObject(" & key & "), invalidated")
                                        Exit For
                                    End If
                                Next
                            End If
                            If Not invalidate Then
                                appendCacheLog("GetObject(" & key & "), valid")
                                If TypeOf cacheData.data Is resultType Then
                                    returnObject = DirectCast(cacheData.data, resultType)
                                    'return_cacheHit = True
                                End If
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
        ''' The default invalidation date, calculated as the current date plus the invalidationDaysDefault, plus a random number of hours, 0-24
        ''' </summary>
        ''' <returns></returns>
        '
        Public ReadOnly Property invalidationDateDefault As Date
            Get
                Return Now.AddDays(invalidationDaysDefault + Rnd())
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' get the cachename for the invalidation date for a tag
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <returns></returns>
        Private Function getTagInvalidationDateCacheName(tag As String) As String
            Return "TagInvalidationDate-" & tag.ToLower()
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