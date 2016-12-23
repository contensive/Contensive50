
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core
    '
    Public Class coreCacheClass
        Implements IDisposable
        '
        ' store just for live of object. Cannot load in constructor without global code update
        '
        Private cpCore As cpCoreClass
        Private cacheClient As Enyim.Caching.MemcachedClient
        Private rightNow As Date
        Private cacheLogPrefix As String
        '
        Private Const invalidationDaysDefault As Double = 365
        Private cache_globalInvalidationDate As Date = Date.MinValue
        Private cacheForceLocal As Boolean
        '
        '
        <Serializable()>
        Public Class cacheDataClass
            Public tagList As New List(Of String)
            Public saveDate As Date
            Public invalidationDate As Date
            Public data As Object
        End Class

        '
        '====================================================================================================
        '
        Public Function readRaw(ByVal key As String) As Object
            Dim returnObj As Object = Nothing
            Try
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("key cannot be blank")
                Else
                    If (siteProperty_AllowCache_SpecialCaseNotCached) Then
                        If cpCore.cluster.config.isLocalCache Then
                            Throw New NotImplementedException("local cache not implemented yet")
                        Else
                            If cpCore.app.config.enableCache Then
                                returnObj = cacheClient.Get(encodeCacheKey(cpCore.app.config.name & "-" & key))
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Call cpCore.handleLegacyError8("exception")
            End Try
            Return returnObj
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' new constructor. Initializes properties and cache client. For now, called from each public routine. Eventually this is contructor.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Dim logMsg As String = "new cache instance"
            Try
                Me.cpCore = cpCore
                Dim cacheEndpointSplit As String()
                Dim cacheEndpointPort As Integer
                Dim cacheEndpoint As String
                Dim cacheConfig As Amazon.ElastiCacheCluster.ElastiCacheClusterConfig
                '
                rightNow = Now()
                cacheLogPrefix = "cacheLog"
                '
                cacheForceLocal = True
                cacheEndpoint = cpCore.cluster.config.awsElastiCacheConfigurationEndpoint
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
                    cacheForceLocal = False
                End If
                '
                ' initialize cache - load global cache invalidation date
                '
                cache_globalInvalidationDate = EncodeDate(readRaw("globalInvalidationDate"))
                appendCacheLog(logMsg)
            Catch ex As Exception
                '
                appendCacheLog(logMsg & ", exception exit")
                cpCore.handleException(ex, "Exception in cacheClass newNew constructor")
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
                Dim dataObject As Object
                '
                If Not _globalInvalidationDateLoaded Then
                    _globalInvalidationDateLoaded = True
                    dataObject = readRaw("globalInvalidationDate")
                    If TypeOf (dataObject) Is Date Then
                        _globalInvalidationDate = DirectCast(dataObject, Date)
                    ElseIf TypeOf (dataObject) Is String Then
                        _globalInvalidationDate = EncodeDate(dataObject)
                    Else
                        _globalInvalidationDate = New Date(1990, 8, 7)
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
        Private Sub saveRaw(ByVal Key As String, ByVal data As Object, invalidationDate As Date)
            Try
                If (String.IsNullOrEmpty(Key)) Then
                    Throw New ArgumentException("Cache key cannot be blank")
                ElseIf (invalidationDate.CompareTo(now) <= 0) Then
                    Throw New ArgumentException("Cache invalidation date cannot be in the past")
                Else
                    If cacheForceLocal Then
                        Throw New NotImplementedException("Local cache mode is not implemented yet")
                        'cp.Cache.Save(encodeCacheKey(Key), dataString, , invalidationDate)
                    Else
                        Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, encodeCacheKey(cpCore.app.config.name & "-" & Key), data, invalidationDate)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Private Sub save(ByVal Key As String, ByVal cacheData As cacheDataClass, Optional invalidationDate As Date = #12:00:00 AM#)
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
                    saveRaw(Key, cacheData, invalidationDate)
                Else
                    '
                    ' invalidate this key
                    '
                    saveRaw(Key, Nothing, invalidationDateDefault)
                End If

                If (String.IsNullOrEmpty(Key)) Then
                    cpCore.handleException(New Exception("key cannot be empty"))
                ElseIf (invalidationDate <= Now()) Then
                    cpCore.handleException(New Exception("invalidationDate must be > current date/time"))
                Else
                    allowSave = False
                    If cacheData Is Nothing Then
                        allowSave = True
                    ElseIf Not cacheData.GetType.IsSerializable Then
                        cpCore.handleException(New Exception("Data object must be serializable"))
                    Else
                        allowSave = True
                    End If
                    If allowSave Then
                        saveRaw(Key, cacheData, invalidationDate)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Public Sub save(key As String, cacheObject As Object)
            Try
                Call save(key, cacheObject, invalidationDateDefault, New List(Of String))
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
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="invalidationTagList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub save(key As String, cacheObject As Object, invalidationDate As Date, invalidationTagList As List(Of String))
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
                    cacheData.saveDate = Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = invalidationTagList
                    '
                    saveRaw(key, cacheData, invalidationDate)
                End If
            Catch ex As Exception
                Try
                    cpCore.handleException(ex)
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
        Public Sub save(key As String, cacheObject As Object, invalidationTagList As List(Of String))
            Try
                Call save(key, cacheObject, invalidationDateDefault, invalidationTagList)
            Catch ex As Exception
                Try
                    cpCore.handleException(ex)
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
        Public Sub save(key As String, cacheObject As Object, invalidationTagCommaList As String)
            Try
                Dim invalidationTagList As New List(Of String)
                '
                invalidationTagList.AddRange(invalidationTagCommaList.Split(","c))                '
                '
                Call save(key, cacheObject, invalidationDateDefault, invalidationTagList)
            Catch ex As Exception
                Try
                    cpCore.handleException(ex)
                Catch errObj As Exception
                End Try
            End Try
        End Sub
        '


        ''
        ''====================================================================================================
        '''' <summary>
        '''' read a cache name and return it serialized as a string, for compatibility with existing site.
        '''' </summary>
        '''' <param name="CP"></param>
        '''' <param name="cacheName"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Function read(ByVal CP As Contensive.BaseClasses.CPBaseClass, cacheName As String) As String
        '    Dim returnResult As String = ""
        '    Dim logMsg As String = vbTab & "read(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")"
        '    Try
        '        '
        '        Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
        '        Dim cacheData As cacheDataClass
        '        Dim tagInvalidated As Boolean
        '        If allowCache Then
        '            'addCacheNameToList(CP, cacheName)
        '            cacheData = read3(cacheName)
        '            If (cacheData Is Nothing) Then
        '                logMsg &= ", CACHE-MISS"
        '            Else
        '                If (globalInvalidationDate > cacheData.saveDate) Or (cacheData.invalidationDate < rightNow) Then
        '                    If (globalInvalidationDate > cacheData.saveDate) Then
        '                        logMsg &= ", CACHE-INVALIDATED by globalInvalidationDate"
        '                    Else
        '                        logMsg &= ", CACHE-INVALIDATED by data invalidationDate"
        '                    End If
        '                Else
        '                    '
        '                    ' if this data is newer that the last glocal invalidation, continue
        '                    '
        '                    For Each tag As String In cacheData.tagList
        '                        If (getTagInvalidationDate(tag) > cacheData.saveDate) Then
        '                            tagInvalidated = True
        '                            logMsg &= ", CACHE-INVALIDATED by tag invalidationDate (" & tag & ")"
        '                            Exit For
        '                        End If
        '                    Next
        '                    If Not tagInvalidated Then
        '                        '
        '                        ' no tags are invalidated, return the data
        '                        '
        '                        If TypeOf (cacheData.data) Is String Then
        '                            returnResult = DirectCast(cacheData.data, String)
        '                        Else
        '                            returnResult = json_serializer.Serialize(cacheData.data)
        '                            's = Newtonsoft.Json.JsonConvert.SerializeObject(cacheData.data)
        '                        End If
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        appendCacheLog(logMsg & ", length(" & returnResult.Length & ")")
        '        '
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnResult
        'End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' read a cache name and return the object.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="key">The cache entry name, a-z, 0-9, no spaces only</param>
        ''' <param name="return_cacheHit">Returns true if a valid cache entry found, false if there was no valid cache stored.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function read(key As String) As Object
            Dim returnData As Object = Nothing
            'Dim logMsg As String = vbTab & "read2(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")"
            Try
                Dim cacheData As cacheDataClass
                Dim cacheDataObject As Object
                Dim tagInvalidated As Boolean
                '
                If allowCache Then
                    cacheDataObject = readRaw(key)
                    '
                    ' cast object as cacheDataClass
                    '
                    If Not cacheDataObject Is Nothing Then
                        If (TypeOf cacheDataObject Is cacheDataClass) Then
                            cacheData = DirectCast(cacheDataObject, cacheDataClass)
                            If (globalInvalidationDate > cacheData.saveDate) Or (cacheData.invalidationDate < rightNow) Then
                                If (globalInvalidationDate > cacheData.saveDate) Then
                                    'logMsg &= ", CACHE-INVALIDATED by globalInvalidationDate"
                                Else
                                    'logMsg &= ", CACHE-INVALIDATED by data invalidationDate"
                                End If
                            Else
                                '
                                ' if this data is newer that the last glocal invalidation, continue
                                '
                                For Each tag As String In cacheData.tagList
                                    If (getTagInvalidationDate(tag) >= cacheData.saveDate) Then
                                        tagInvalidated = True
                                        'logMsg &= ", CACHE-INVALIDATED by tag invalidationDate (" & tag & ")"
                                        Exit For
                                    End If
                                Next
                                If Not tagInvalidated Then
                                    '
                                    ' no tags are invalidated, return the data
                                    '
                                    returnData = cacheData.data
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnData
        End Function

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
                invalidateAll(CP)
                CP.Cache.ClearAll()
            Catch ex As Exception
                Try
                    cpCore.handleException(ex)
                Catch errObj As Exception
                End Try
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
            Dim returnTagInvalidationDate As Date = New Date(1990, 8, 7)
            Try
                Dim cacheNameTagInvalidateDate As String = "tagInvalidationDate-" & tag
                Dim cacheObject As Object
                '
                If allowCache Then
                    If Not String.IsNullOrEmpty(tag) Then
                        '
                        ' get it from raw cache
                        '
                        cacheObject = readRaw(cacheNameTagInvalidateDate)
                        If TypeOf (cacheObject) Is String Then
                            returnTagInvalidationDate = EncodeDate(cacheObject)
                        ElseIf TypeOf (cacheObject) Is Date Then
                            returnTagInvalidationDate = DirectCast(cacheObject, Date)
                        Else
                            returnTagInvalidationDate = New Date(1990, 8, 7)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnTagInvalidationDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates the entire cache (except those entires written with saveRaw)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub invalidateAll(cp As Contensive.BaseClasses.CPBaseClass)
            Try
                '
                appendCacheLog(vbTab & "invalidateAll(" & cp.Doc.StartTime & ")")
                '
                Call saveRaw("globalInvalidationDate", Now().ToString(), Now().AddYears(10))
                _globalInvalidationDateLoaded = False
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a tag
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <remarks></remarks>
        Public Sub invalidateTag(cp As Contensive.BaseClasses.CPBaseClass, ByVal tag As String)
            Try
                '
                appendCacheLog(vbTab & "invalidateTag(" & cp.Doc.StartTime & ")")
                '
                Dim cacheName As String = "tagInvalidationDate-"
                '
                If allowCache Then
                    If String.IsNullOrEmpty(tag) Then
                        saveRaw(cacheName & tag, Now.ToString, Now.AddYears(10))
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Clear a Content definitions TimeStamp, and all of its child's also
        '========================================================================
        '
        Public Sub invalidateTag(ByVal tag As String)
            Try
                Dim cacheName As String = "tagInvalidationDate-"
                Dim cdef As coreMetaDataClass.CDefClass
                '
                If cpCore.app.config.enableCache Then
                    If tag <> "" Then
                        '
                        ' set the tags invalidation date
                        '
                        saveRaw2(cacheName & tag, Now.ToString)
                        '
                        ' test if this tag is a content name
                        '
                        cdef = cpCore.app.metaData.getCdef(tag)
                        If Not cdef Is Nothing Then
                            '
                            ' Invalidate all child cdef
                            '
                            If cdef.childIdList.Count > 0 Then
                                For Each childId As Integer In cdef.childIdList
                                    Dim childCdef As coreMetaDataClass.CDefClass
                                    childCdef = cpCore.app.metaData.getCdef(childId)
                                    If Not childCdef Is Nothing Then
                                        saveRaw2(cacheName & childCdef.Name, Now.ToString)
                                    End If
                                Next
                            End If
                            '
                            ' Now go up to the top-most parent
                            '
                            Do While cdef.parentID > 0
                                'Dim parentCdef As metaDataClass.CDefClass
                                cdef = cpCore.app.metaData.getCdef(cdef.parentID)
                                If Not cdef Is Nothing Then
                                    saveRaw2(cacheName & cdef.Name, Now.ToString)
                                End If
                            Loop
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Clears all baked content in any content definition specified
        '========================================================================
        '
        Public Sub invalidateTagList2(ByVal tagList As String)
            Try
                Dim tags() As String
                Dim tag As String
                Dim Pointer As Integer
                '
                If cpCore.app.config.enableCache Then
                    If tagList <> "" Then
                        tags = Split(tagList, ",")
                        For Pointer = 0 To UBound(tags)
                            tag = tags(Pointer)
                            If tag <> "" Then
                                Call invalidateTag(tag)
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a list of tags 
        ''' </summary>
        ''' <param name="tagList"></param>
        ''' <remarks></remarks>
        Public Sub invalidateTagList(cp As Contensive.BaseClasses.CPBaseClass, ByVal tagList As List(Of String))
            Try
                '
                appendCacheLog(vbTab & "invalidateTag(" & cp.Doc.StartTime & ")")
                '
                If allowCache Then
                    For Each tag In tagList
                        Call invalidateTag(cp, tag)
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Private Function encodeCacheKey(key As String) As String
            Dim returnKey As String
            returnKey = Regex.Replace(key, "0x[a-fA-F\d]{2}", "_")
            returnKey = returnKey.Replace(" ", "_")
            Return returnKey
        End Function
        '
        '=======================================================================
        Private Sub appendCacheLog(line As String)
            Try
                cpCore.appendLog("pid(" & Process.GetCurrentProcess().Id.ToString.PadLeft(4, "0"c) & "),thread(" & Threading.Thread.CurrentThread.ManagedThreadId.ToString.PadLeft(4, "0"c) & ")" & vbTab & line, cacheLogPrefix)
            Catch ex As Exception
                ' ignore logging errors
            End Try
        End Sub
        '
        Private ReadOnly Property allowCache As Boolean
            Get
                Return cpCore.app.config.enableCache
            End Get
        End Property
        '
        '
        '
        Public Sub saveRaw3(ByVal Key As String, ByVal data As Object, Optional invalidationDate As Date = #12:00:00 AM#)
            Try
                'Dim testValue As Object
                If invalidationDate <= Date.MinValue Then
                    invalidationDate = Now.AddDays(7 + Rnd())
                End If
                If (Key = "") Then
                    cpCore.handleException(New ApplicationException("key cannot be empty"))
                ElseIf (invalidationDate <= Now()) Then
                    cpCore.handleException(New ApplicationException("invalidationDate must be > current date/time"))
                Else
                    Dim allowSave As Boolean
                    allowSave = False
                    If data Is Nothing Then
                        allowSave = True
                    ElseIf Not data.GetType.IsSerializable Then
                        cpCore.handleException(New ApplicationException("data object must be serializable"))
                    Else
                        allowSave = True
                    End If
                    If allowSave Then
                        If cpCore.cluster.config.isLocalCache Then
                            Throw New NotImplementedException("local cache not implemented yet")
                        Else
                            Call cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, encodeCacheKey(Key), data, invalidationDate)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub

        '
        '
        '
        Public Sub saveRaw2(ByVal Key As String, ByVal data As Object, Optional invalidationDate As Date = #12:00:00 AM#)
            Try
                If cpCore.app.config.enableCache Then
                    If (invalidationDate <= Date.MinValue) Then
                        invalidationDate = Now.AddDays(7.0# + Rnd())
                    End If
                    Call saveRaw3(cpCore.app.config.name & "-" & Key, data, invalidationDate)
                End If
            Catch ex As Exception
                Call cpCore.handleLegacyError8("exception")
            End Try
            '            On Error GoTo ErrorTrap
            '            '
            '            Dim Ptr As Integer
            '            '
            '            If (Key = "") Then
            '                Call csv_HandleClassErrorAndResume("cache_save", "Can not call cache_save with empty key")
            '            Else
            '                If InStr(1, "," & docCacheUsedKeyList & ",", "," & Key & ",", vbTextCompare) <> 0 Then
            '                    '
            '                    ' the key is there already, remove it and add the new one
            '                    '
            '                    On Error Resume Next
            '                    Call docCache.Remove(Key)
            '                    If Err.Number <> 0 Then
            '                        Call csv_HandleClassErrorAndResume("cache_save", "Error during collection remove method for key [" & Key & "], err " & GetErrString(Err))
            '                        Err.Clear()
            '                    End If
            '                    Call docCache.Add(Var, Key)
            '                    If Err.Number <> 0 Then
            '                        Call csv_HandleClassErrorAndResume("cache_save", "Error during collection Add method for key [" & Key & "], err " & GetErrString(Err))
            '                        Err.Clear()
            '                    End If
            '                    On Error GoTo ErrorTrap
            '                Else
            '                    '
            '                    ' not there yet, just add it
            '                    '
            '                    docCacheUsedKeyList = docCacheUsedKeyList & "," & Key
            '                    Call docCache.Add(Var, Key)
            '                End If
            '            End If
            '            '
            '            Exit Sub
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassErrorAndResume("cache_save", "errorTrap")
        End Sub
        '
        '========================================================================
        '   Clear Cache
        '========================================================================
        '
        Public Sub invalidateAll2()
            Try
                If cpCore.app.config.enableCache Then
                    cache_globalInvalidationDate = Now
                    Call saveRaw2("globalInvalidationDate", cache_globalInvalidationDate, Now.AddYears(1))
                    '
                    '' the rest of this should use cache_save() which is handled by the global invalidation
                    ''
                    'Call cpCore.cache_addon_clear()
                    'Call cpCore.cache_linkAlias_clear()
                    'Call cpCore.pageManager_cache_pageContent_clear()
                    'Call cpCore.pageManager_cache_pageTemplate_clear()
                    'Call cpCore.pageManager_cache_siteSection_clear()
                    'If Not IsNothing(cpCore.cache_addonStyleRules) Then
                    '    cpCore.cache_addonStyleRules.clear()
                    'End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        ''
        ''========================================================================
        ''   Create the Content Time Stamp
        ''       "ContentName:DateTimeStampCreated"
        ''
        ''       Updated 1/20/2008 - Querying the Db for the count is very slow for large tables (ccMember). Instead of
        ''       getting Db info, just save the current time.
        ''       Since the ContentTimeStamp (CTS) is cached in appServices, it only falls through to the Db check on
        ''       restart, or if a ClearTimeStamp is called. In both cases, it is OK to return Back empty. A re-Bake in
        ''       can not be too much more time consuming then the count of members (15 seconds)
        ''
        ''       was:
        ''       This is a string that changes if any record in the Content Definition is changed
        ''       "ContentName:NumberOfRecords,LastModifiedDate;"
        ''========================================================================
        ''
        'Private Function xgetSetContentTimeStamp(ByVal ContentName As String) As String
        '    Dim returnTimeStamp As String = ""
        '    Try
        '        Dim Criteria As String
        '        Dim SQL As String
        '        Dim CDef As CDefType
        '        '
        '        returnTimeStamp = getContentTimeStamp(ContentName)
        '        If returnTimeStamp = "" Then
        '            returnTimeStamp = ContentName & ":" & EncodeText(Now())
        '            Call SetContentTimeStamp(ContentName, returnTimeStamp)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnTimeStamp
        'End Function
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
        Public Function read(Of resultType)(ByVal key As String) As Object
            Dim returnObject As Object = Nothing
            Try
                Dim cacheData As cacheDataClass
                Dim invalidate As Boolean = False
                '
                'return_cacheHit = False
                If (String.IsNullOrEmpty(key)) Then
                    Throw New ArgumentException("cache.read, key cannot be blank.")
                Else
                    '
                    ' block all cache if editing or rendering edits
                    '
                    cacheData = DirectCast(readRaw(key), cacheDataClass)
                    If Not (cacheData Is Nothing) Then
                        If cache_globalInvalidationDate < cacheData.saveDate Then
                            '
                            ' if this data is newer that the last global invalidation, continue
                            '
                            Dim tagInvalidationDate As Date
                            For Each tag As String In cacheData.tagList
                                tagInvalidationDate = getTagInvalidationDate(tag)
                                If (tagInvalidationDate >= cacheData.saveDate) Then
                                    invalidate = True
                                    Exit For
                                End If
                            Next
                            If Not invalidate Then
                                If TypeOf cacheData.data Is resultType Then
                                    returnObject = cacheData.data
                                    'return_cacheHit = True
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnObject
        End Function
        '
        '========================================================================
        '   Saves a string to the Bake Cache
        '
        '   Creates the depency string from the tablenamelist
        '========================================================================
        '
        Public Sub cachexSaveRename(ByVal key As String, ByVal data As Object, Optional ByVal tagList As String = "", Optional ByVal invalidationDate As Date = #12:00:00 AM#)
            Try
                Dim cacheData As New cacheDataClass
                '
                If cpCore.app.config.enableCache Then
                    If (siteProperty_AllowCache_SpecialCaseNotCached) Then
                        '
                        ' user_isEditing and visitProperty_allowWorkflowRendering have to be checked where they are used, not globally
                        '   1) site properties are cached, so the cache cannot check site properties (cyclic calls)
                        '   2) cache calls are made for many more things that do not depend on editing
                        ' allowCache cant be a site property because it sp are cached (cyclic again)
                        '
                        'If (siteProperty_AllowCache_SpecialCaseNotCached) And (Not cpCore.user_isEditingAnything) And (Not cpCore.main_VisitProperty_AllowWorkflowRendering) Then
                        If (invalidationDate <= Date.MinValue) Then
                            invalidationDate = Now.AddDays(7.0# + Rnd())
                        End If
                        cacheData.data = data
                        If Not String.IsNullOrEmpty(tagList) Then
                            cacheData.tagList.AddRange(tagList.Split(CChar(",")))
                        End If
                        cacheData.saveDate = Now()
                        Call saveRaw2(key, cacheData, invalidationDate)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub




#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                '
                appendCacheLog("disposing")
                '
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                    If Not (cacheClient Is Nothing) Then
                        '
                        appendCacheLog("dispose mc")
                        '
                        cacheClient.Dispose()
                    Else
                        '
                        appendCacheLog("dispose NO mc")
                        '
                    End If
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowCache_SpecialCaseNotCached() As Boolean
            Get
                '
                ' site property now runs query
                '
                'Return True
                Dim propertyFound As Boolean = False
                If Not siteProperty_AllowCache_LocalLoaded Then
                    siteProperty_AllowCache_Local = EncodeBoolean(cpCore.app.siteProperty_getText_noCache("AllowBake", "0", propertyFound))
                    siteProperty_AllowCache_LocalLoaded = True
                End If
                siteProperty_AllowCache_SpecialCaseNotCached = siteProperty_AllowCache_Local

            End Get
        End Property
        Private siteProperty_AllowCache_LocalLoaded As Boolean = False
        Private siteProperty_AllowCache_Local As Boolean
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


        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' finalize
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
    '
End Namespace