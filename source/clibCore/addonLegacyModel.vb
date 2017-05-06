
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ''' <summary>
    ''' Addon cache. This class is model addon objects
    ''' </summary>
    Public Class addonLegacyModel
        '
        ' not IDisposable - not contained classes that need to be disposed
        '
        Private cpCore As coreClass
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        ' ----- addon cache
        '
        Public Const cacheName = "cache_addon"
        Public Const cache_addon2_fieldList = "id,active,name,ccguid,collectionid,Copy,ccguid,Link,ObjectProgramID,DotNetClass,ArgumentList,CopyText,IsInline,BlockDefaultStyles,StylesFilename,CustomStylesFilename,formxml,RemoteAssetLink,AsAjax,InFrame,ScriptingEntryPoint,ScriptingLanguageID,ScriptingCode,BlockEditTools,ScriptingTimeout,inlineScript,help,helplink,JavaScriptOnLoad,JavaScriptBodyEnd,PageTitle,MetaDescription,MetaKeywordList,OtherHeadTags,JSFilename,remoteMethod,onBodyStart,onBodyEnd,OnPageStartEvent,OnPageEndEvent,robotsTxt"
        Public Class addonClass
            Public addonCache_Id As Integer
            Public addonCache_active As Boolean
            Public addonCache_name As String
            Public addonCache_guid As String
            Public addonCache_collectionid As Integer
            Public addonCache_Copy As String
            Public addonCache_ccguid As String
            Public addonCache_Link As String
            Public addonCache_ObjectProgramID As String
            Public addonCache_DotNetClass As String
            Public addonCache_ArgumentList As String
            Public addonCache_CopyText As String
            Public addonCache_IsInline As Boolean
            Public addonCache_BlockDefaultStyles As Boolean
            Public addonCache_StylesFilename As String
            Public addonCache_CustomStylesFilename As String
            Public addonCache_formxml As String
            Public addonCache_RemoteAssetLink As String
            Public addonCache_AsAjax As Boolean
            Public addonCache_InFrame As Boolean
            Public addonCache_ScriptingEntryPoint As String
            Public addonCache_ScriptingLanguageID As Integer
            Public addonCache_ScriptingCode As String
            Public addonCache_BlockEditTools As Boolean
            Public addonCache_ScriptingTimeout As String
            Public addonCache_inlineScript As String
            Public addonCache_help As String
            Public addonCache_helpLink As String
            Public addonCache_JavaScriptOnLoad As String
            Public addonCache_JavaScriptBodyEnd As String
            Public addonCache_PageTitle As String
            Public addonCache_MetaDescription As String
            Public addonCache_MetaKeywordList As String
            Public addonCache_OtherHeadTags As String
            Public addonCache_JSFilename As String
            Public addonCache_remoteMethod As Boolean
            Public addoncache_OnBodyStart As Boolean
            Public addoncache_OnBodyEnd As Boolean
            Public addoncache_OnPageStart As Boolean
            Public addoncache_OnPageEnd As Boolean
            Public addoncache_robotsTxt As String
        End Class
        '
        <Serializable>
        Public Class addonsCacheClass
            Public addonList As Dictionary(Of String, addonClass)
            Public idIndex As coreKeyPtrIndexClass
            Public nameIndex As coreKeyPtrIndexClass
            Public guidIndex As coreKeyPtrIndexClass
            Public onBodyStartPtrs As Integer()
            Public onBodyEndPtrs As Integer()
            Public onPageStartPtrs As Integer()
            Public onPageEndPtrs As Integer()
            Public robotsTxt As String = ""
            '
            Public propertyBag_idIndex As String
            Public propertyBag_nameIndex As String
            Public propertyBag_guidIndex As String
        End Class
        '
        Friend localCache As addonsCacheClass
        '
        '====================================================================================================
        ''' <summary>
        ''' clear the addonCAche
        ''' </summary>
        Public Sub clear()
            Try
                If localCache Is Nothing Then
                    localCache = New addonsCacheClass()
                End If
                localCache.addonList = New Dictionary(Of String, addonClass)
                localCache.guidIndex = New coreKeyPtrIndexClass
                localCache.idIndex = New coreKeyPtrIndexClass
                localCache.nameIndex = New coreKeyPtrIndexClass
                localCache.onBodyEndPtrs = {}
                localCache.onBodyStartPtrs = {}
                localCache.onPageEndPtrs = {}
                localCache.onPageStartPtrs = {}
                localCache.propertyBag_guidIndex = ""
                localCache.propertyBag_idIndex = ""
                localCache.propertyBag_nameIndex = ""
                Call cpCore.cache.setKey(cacheName, localCache, cnAddons)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' update a row in the addonCache
        ''' </summary>
        ''' <param name="RecordID"></param>
        Public Sub updateRow(RecordID As Integer)
            Try
                Dim dt As DataTable
                Dim addon As addonClass
                Dim RecordName As String
                Dim Id As Integer
                Dim addonCacheRow As String(,)
                Dim addonPtr As Integer
                Dim addonCacheRowOK As Boolean
                Dim SQL As String
                '
                If RecordID <= 0 Then
                    Throw New ArgumentException("argument invalid, recordid=[" & RecordID & "]")
                Else
                    Call load()
                    If localCache.addonList.Count > 0 Then
                        'hint = hint & ", 3 cache_addonCnt=[" & cache_addons.addonList.Count & "]"
                        For addonPtr = 0 To localCache.addonList.Count - 1
                            If genericController.EncodeInteger(localCache.addonList(addonPtr.ToString).addonCache_Id) = RecordID Then
                                Exit For
                            End If
                        Next
                        addonCacheRowOK = False
                        SQL = "select " & cache_addon2_fieldList & " from ccaggregatefunctions where id=" & RecordID
                        addon = New addonClass
                        dt = cpCore.db.executeSql(SQL)
                        If dt.Rows.Count > 0 Then
                            With dt.Rows(0)
                                addon.addonCache_Id = genericController.EncodeInteger(.Item(0))
                                addon.addonCache_active = genericController.EncodeBoolean(.Item(1))
                                addon.addonCache_name = genericController.encodeText(.Item(2))
                                addon.addonCache_guid = genericController.encodeText(.Item(3))
                                addon.addonCache_collectionid = genericController.EncodeInteger(.Item(4))
                                addon.addonCache_Copy = genericController.encodeText(.Item(5))
                                addon.addonCache_ccguid = genericController.encodeText(.Item(6))
                                addon.addonCache_Link = genericController.encodeText(.Item(7))
                                addon.addonCache_ObjectProgramID = genericController.encodeText(.Item(8))
                                addon.addonCache_DotNetClass = genericController.encodeText(.Item(9))
                                addon.addonCache_ArgumentList = genericController.encodeText(.Item(10))
                                addon.addonCache_CopyText = genericController.encodeText(.Item(11))
                                addon.addonCache_IsInline = genericController.EncodeBoolean(.Item(12))
                                addon.addonCache_BlockDefaultStyles = genericController.EncodeBoolean(.Item(13))
                                addon.addonCache_StylesFilename = genericController.encodeText(.Item(14))
                                addon.addonCache_CustomStylesFilename = genericController.encodeText(.Item(15))
                                addon.addonCache_formxml = genericController.encodeText(.Item(16))
                                addon.addonCache_RemoteAssetLink = genericController.encodeText(.Item(17))
                                addon.addonCache_AsAjax = genericController.EncodeBoolean(.Item(18))
                                addon.addonCache_InFrame = genericController.EncodeBoolean(.Item(19))
                                addon.addonCache_ScriptingEntryPoint = genericController.encodeText(.Item(20))
                                addon.addonCache_ScriptingLanguageID = genericController.EncodeInteger(.Item(21))
                                addon.addonCache_ScriptingCode = genericController.encodeText(.Item(22))
                                addon.addonCache_BlockEditTools = genericController.EncodeBoolean(.Item(23))
                                addon.addonCache_ScriptingTimeout = genericController.encodeText(.Item(24))
                                addon.addonCache_inlineScript = genericController.encodeText(.Item(25))
                                addon.addonCache_help = genericController.encodeText(.Item(26))
                                addon.addonCache_helpLink = genericController.encodeText(.Item(27))
                                addon.addonCache_JavaScriptOnLoad = genericController.encodeText(.Item(28))
                                addon.addonCache_JavaScriptBodyEnd = genericController.encodeText(.Item(29))
                                addon.addonCache_PageTitle = genericController.encodeText(.Item(30))
                                addon.addonCache_MetaDescription = genericController.encodeText(.Item(31))
                                addon.addonCache_MetaKeywordList = genericController.encodeText(.Item(32))
                                addon.addonCache_OtherHeadTags = genericController.encodeText(.Item(33))
                                addon.addonCache_JSFilename = genericController.encodeText(.Item(34))
                                addon.addonCache_remoteMethod = genericController.EncodeBoolean(.Item(35))
                                addon.addoncache_OnBodyStart = genericController.EncodeBoolean(.Item(36))
                                addon.addoncache_OnBodyEnd = genericController.EncodeBoolean(.Item(37))
                                addon.addoncache_OnPageStart = genericController.EncodeBoolean(.Item(38))
                                addon.addoncache_OnPageEnd = genericController.EncodeBoolean(.Item(39))
                                addon.addoncache_robotsTxt = genericController.encodeText(.Item(40))
                                addon.addonCache_active = genericController.EncodeBoolean(.Item(41))
                            End With
                        End If
                        Call dt.Dispose()
                        'hint = hint & ", 4 RowPtr=[" & addonPtr & "], sql=[" & SQL & "]"
                        addonCacheRow = cpCore.db.convertDataTabletoArray(cpCore.db.executeSql(SQL))

                        'RS = app.csv_OpenRSSQL_Internal("Default", SQL, 120, 1, 1, False, CursorLocationEnum.adUseClient, LockTypeEnum.ADLOCKOptimistic, CursorTypeEnum.ADOPENForwardOnly)
                        'If (isDataTableOk(rs)) Then
                        '    If Not rs.rows.count=0 Then
                        '        addonCacheRow = RS.GetRows
                        '        addonCacheRowOK = True
                        '    End If
                        '    If (isDataTableOk(rs)) Then
                        '        If false Then
                        '            'RS.Close()
                        '        End If
                        '        'RS = Nothing
                        '    End If
                        'End If
                        'hint = hint & ", 5 addonCacheRowOK=[" & addonCacheRowOK & "]"
                        If addonCacheRow.Length > 0 Then
                            If addonPtr = localCache.addonList.Count Then
                                '
                                ' not found in cache - add a new entry
                                '
                                'hint = hint & ", 6"
                                localCache.addonList.Add(localCache.addonList.Count.ToString, addon)
                                'ReDim Preserve cache_addons.addons(rowptr).addonCacheColCnt - 1, cache_addons.addonList.Count - 1)
                            End If
                            '
                            ' Transfer data from Db data to the addonCache
                            '
                            ''hint = hint & ", 7"
                            'For ColPtr = 0 To UBound(cache_addon, 1)
                            '    cache_addons.addons(rowptr).ColPtr, RowPtr) = addonCacheRow(ColPtr, 0)
                            'Next
                            '
                            ' build id and name indexes
                            '
                            'hint = hint & ", 8"
                            Id = localCache.addonList(addonPtr.ToString).addonCache_Id
                            RecordName = localCache.addonList(addonPtr.ToString).addonCache_name
                            '
                            'hint = hint & ", 9"
                            Call localCache.idIndex.setPtr(genericController.encodeText(Id), addonPtr)
                            '
                            If RecordName <> "" Then
                                'hint = hint & ", 10"
                                Call localCache.nameIndex.setPtr(RecordName, addonPtr)
                            End If
                            '
                            'hint = hint & ", 11"
                            Call save()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' getPrt into addonCache
        ''' </summary>
        ''' <param name="addonNameGuidOrId"></param>
        ''' <returns></returns>
        Public Function getPtr(addonNameGuidOrId As String) As Integer
            Dim ReturnPtr As Integer = -1
            Try
                Dim CS As Integer
                Dim sqlCriteria As String
                Dim addonId As Integer
                '
                Call load()
                If localCache.addonList.Count <= 0 Then
                    '
                ElseIf (localCache.idIndex Is Nothing) Then
                    '
                Else
                    If genericController.vbIsNumeric(addonNameGuidOrId) Then
                        ReturnPtr = localCache.idIndex.getPtr(addonNameGuidOrId.ToUpper())
                    End If
                    If ReturnPtr < 0 Then
                        ReturnPtr = localCache.nameIndex.getPtr(addonNameGuidOrId)
                        If ReturnPtr < 0 Then
                            ReturnPtr = localCache.guidIndex.getPtr(CStr(addonNameGuidOrId))
                        End If
                    End If
                    If ReturnPtr >= 0 Then
                        '
                    Else
                        '
                        ' This addonId is missing from cache - try to load it from Db
                        '
                        sqlCriteria = "(id=" & cpCore.db.encodeSQLNumber(EncodeInteger(addonNameGuidOrId)) & ")or(name=" & cpCore.db.encodeSQLText(addonNameGuidOrId) & ")or(ccguid=" & cpCore.db.encodeSQLText(addonNameGuidOrId) & ")"
                        CS = cpCore.db.cs_open(cnAddons, sqlCriteria)
                        If cpCore.db.cs_ok(CS) Then
                            addonId = cpCore.db.cs_getInteger(CS, "id")
                        End If
                        Call cpCore.db.cs_Close(CS)
                        If addonId > 0 Then
                            Call updateRow(addonId)
                            Call load()
                            If localCache.addonList.Count <= 0 Then
                                '
                            ElseIf (localCache.idIndex Is Nothing) Then
                                '
                            Else
                                ReturnPtr = localCache.idIndex.getPtr(CStr(addonId))
                            End If
                        End If
                    End If
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return ReturnPtr
        End Function
        '
        '================================================================================================
        ''' <summary>
        ''' load addonCache
        ''' </summary>
        Public Sub load()
            Try
                Dim addonPtr As Integer
                Dim SQL As String
                Dim RecordID As Integer
                Dim RecordName As String
                Dim RecordGuid As String
                Dim cacheTest As Object
                Dim onBodyStartCnt As Integer
                Dim onBodyEndCnt As Integer
                Dim onPageStartCnt As Integer
                Dim onPageEndCnt As Integer
                Dim robotsTxt As String
                Dim needLoad As Boolean
                Dim onBodyStart As Boolean
                Dim onBodyEnd As Boolean
                Dim onPageStart As Boolean
                Dim onPageEnd As Boolean
                '
                needLoad = False
                If localCache Is Nothing Then
                    needLoad = True
                ElseIf localCache.addonList Is Nothing Then
                    needLoad = True
                ElseIf localCache.addonList.Count = 0 Then
                    needLoad = True
                End If
                If needLoad Then
                    '
                    ' Load cached addonCache
                    '
                    Try
                        cacheTest = cpCore.cache.getObject(Of addonsCacheClass)(cacheName)
                        Dim testCache1 = cpCore.cache.getObject(Of addonsCacheClass)("testCache1")
                        Dim testCache2 = cpCore.cache.getObject(Of addonsCacheClass)("testCache2")
                        Dim testCache3 = cpCore.cache.getObject(Of String)("testCache3")
                        If Not IsNothing(cacheTest) Then
                            localCache = DirectCast(cacheTest, addonsCacheClass)
                            If Not IsNothing(localCache) Then
                                If localCache.addonList.Count > 0 Then
                                    localCache.idIndex.importPropertyBag(localCache.propertyBag_idIndex)
                                    localCache.nameIndex.importPropertyBag(localCache.propertyBag_nameIndex)
                                    localCache.guidIndex.importPropertyBag(localCache.propertyBag_guidIndex)
                                    needLoad = False
                                End If
                            End If
                        End If
                    Catch ex As Exception
                        ' did not load
                    End Try
                    If needLoad Then
                        '
                        ' cache is empty, build it from scratch
                        '
                        localCache = New addonsCacheClass
                        localCache.addonList = New Dictionary(Of String, addonClass)
                        localCache.guidIndex = New coreKeyPtrIndexClass
                        localCache.idIndex = New coreKeyPtrIndexClass
                        localCache.nameIndex = New coreKeyPtrIndexClass
                        onBodyStartCnt = 0
                        onBodyEndCnt = 0
                        SQL = "select " & cache_addon2_fieldList & " from ccAggregateFunctions where (active<>0) order by id"
                        'hint = hint & ", addonCache IsEmpty - Db select"
                        Using dt As DataTable = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count > 0 Then
                                addonPtr = 0
                                For Each dr As DataRow In dt.Rows
                                    Dim addon As New addonClass
                                    RecordID = genericController.EncodeInteger(dr.Item("Id"))
                                    RecordName = genericController.encodeText(dr.Item("name"))
                                    RecordGuid = genericController.encodeText(dr.Item("ccguid"))
                                    onBodyStart = genericController.EncodeBoolean(dr.Item("OnBodyStart"))
                                    onBodyEnd = genericController.EncodeBoolean(dr.Item("OnBodyEnd"))
                                    onPageStart = genericController.EncodeBoolean(dr.Item("onPageStartEvent"))
                                    onPageEnd = genericController.EncodeBoolean(dr.Item("onPageEndEvent"))
                                    robotsTxt = genericController.encodeText(dr.Item("robotsTxt"))
                                    addon.addonCache_active = genericController.EncodeBoolean(dr.Item("active"))
                                    addon.addonCache_ArgumentList = genericController.encodeText(dr.Item("ArgumentList"))
                                    addon.addonCache_AsAjax = genericController.EncodeBoolean(dr.Item("AsAjax"))
                                    addon.addonCache_BlockDefaultStyles = genericController.EncodeBoolean(dr.Item("BlockDefaultStyles"))
                                    addon.addonCache_BlockEditTools = genericController.EncodeBoolean(dr.Item("BlockEditTools"))
                                    addon.addonCache_ccguid = genericController.encodeText(dr.Item("ccguid"))
                                    addon.addonCache_collectionid = genericController.EncodeInteger(dr.Item("collectionid"))
                                    addon.addonCache_Copy = genericController.encodeText(dr.Item("Copy"))
                                    addon.addonCache_CopyText = genericController.encodeText(dr.Item("CopyText"))
                                    addon.addonCache_CustomStylesFilename = genericController.encodeText(dr.Item("CustomStylesFilename"))
                                    addon.addonCache_DotNetClass = genericController.encodeText(dr.Item("DotNetClass"))
                                    addon.addonCache_formxml = genericController.encodeText(dr.Item("formxml"))
                                    addon.addonCache_guid = RecordGuid
                                    addon.addonCache_help = genericController.encodeText(dr.Item("help"))
                                    addon.addonCache_helpLink = genericController.encodeText(dr.Item("helpLink"))
                                    addon.addonCache_Id = RecordID
                                    addon.addonCache_InFrame = genericController.EncodeBoolean(dr.Item("InFrame"))
                                    addon.addonCache_inlineScript = genericController.encodeText(dr.Item("inlineScript"))
                                    addon.addonCache_IsInline = genericController.EncodeBoolean(dr.Item("IsInline"))
                                    addon.addonCache_JavaScriptBodyEnd = genericController.encodeText(dr.Item("JavaScriptBodyEnd"))
                                    addon.addonCache_JavaScriptOnLoad = genericController.encodeText(dr.Item("JavaScriptOnLoad"))
                                    addon.addonCache_JSFilename = genericController.encodeText(dr.Item("JSFilename"))
                                    addon.addonCache_Link = genericController.encodeText(dr.Item("Link"))
                                    addon.addonCache_MetaDescription = genericController.encodeText(dr.Item("MetaDescription"))
                                    addon.addonCache_MetaKeywordList = genericController.encodeText(dr.Item("MetaKeywordList"))
                                    addon.addonCache_name = RecordName
                                    addon.addonCache_ObjectProgramID = genericController.encodeText(dr.Item("ObjectProgramID"))
                                    addon.addoncache_OnBodyEnd = onBodyEnd
                                    addon.addoncache_OnBodyStart = onBodyStart
                                    addon.addoncache_OnPageEnd = onPageEnd
                                    addon.addoncache_OnPageStart = onPageStart
                                    addon.addonCache_OtherHeadTags = genericController.encodeText(dr.Item("OtherHeadTags"))
                                    addon.addonCache_PageTitle = genericController.encodeText(dr.Item("PageTitle"))
                                    addon.addonCache_RemoteAssetLink = genericController.encodeText(dr.Item("RemoteAssetLink"))
                                    addon.addonCache_remoteMethod = genericController.EncodeBoolean(dr.Item("remoteMethod"))
                                    addon.addoncache_robotsTxt = robotsTxt
                                    addon.addonCache_ScriptingCode = genericController.encodeText(dr.Item("ScriptingCode"))
                                    addon.addonCache_ScriptingEntryPoint = genericController.encodeText(dr.Item("ScriptingEntryPoint"))
                                    addon.addonCache_ScriptingLanguageID = genericController.EncodeInteger(dr.Item("ScriptingLanguageID"))
                                    addon.addonCache_ScriptingTimeout = genericController.encodeText(dr.Item("ScriptingTimeout"))
                                    addon.addonCache_StylesFilename = genericController.encodeText(dr.Item("StylesFilename"))
                                    localCache.addonList.Add(addonPtr.ToString, addon)
                                    '
                                    'hint = hint & ",1"
                                    Call localCache.idIndex.setPtr(genericController.encodeText(RecordID), addonPtr)
                                    If RecordName <> "" Then
                                        Call localCache.nameIndex.setPtr(RecordName, addonPtr)
                                    End If
                                    If RecordGuid <> "" Then
                                        Call localCache.guidIndex.setPtr(RecordGuid, addonPtr)
                                    End If
                                    If onBodyStart Then
                                        ' save Id for now, lookup before save
                                        ReDim Preserve localCache.onBodyStartPtrs(onBodyStartCnt)
                                        localCache.onBodyStartPtrs(onBodyStartCnt) = RecordID
                                        onBodyStartCnt = onBodyStartCnt + 1
                                    End If
                                    If onBodyEnd Then
                                        ReDim Preserve localCache.onBodyEndPtrs(onBodyEndCnt)
                                        localCache.onBodyEndPtrs(onBodyEndCnt) = RecordID
                                        onBodyEndCnt = onBodyEndCnt + 1
                                    End If
                                    If onPageStart Then
                                        ReDim Preserve localCache.onPageStartPtrs(onPageStartCnt)
                                        localCache.onPageStartPtrs(onPageStartCnt) = RecordID
                                        onPageStartCnt = onPageStartCnt + 1
                                    End If
                                    'hint = hint & ",2"
                                    If onPageEnd Then
                                        ReDim Preserve localCache.onPageEndPtrs(onPageEndCnt)
                                        localCache.onPageEndPtrs(onPageEndCnt) = RecordID
                                        onPageEndCnt = onPageEndCnt + 1
                                    End If
                                    'hint = hint & ",3"
                                    'robotsTxt = genericController.encodeText(cache_addons.addonList(addonPtr).addoncache_robotsTxt)
                                    'hint = hint & ",4"
                                    If robotsTxt <> "" Then
                                        'hint = hint & ",5"
                                        localCache.robotsTxt = localCache.robotsTxt & vbCrLf & robotsTxt
                                    End If
                                    'hint = hint & ",6"
                                    addonPtr += 1
                                Next
                            End If
                        End Using
                        If localCache.addonList.Count > 0 Then
                            save()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                'Call main_HandleClassError_RevArgs(Err.Number, Err.Source, Err.Description & " Hint=[" & hint & "]", "cache_addon_load", True, False)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save addonCache
        ''' </summary>
        Public Sub save()
            Try
                ' access keyPtr indexes to clear dirty flag and export property bad (remove when the class will serialize)
                '
                Call localCache.idIndex.getPtr("test")
                localCache.propertyBag_idIndex = localCache.idIndex.exportPropertyBag()
                '
                Call localCache.nameIndex.getPtr("test")
                localCache.propertyBag_nameIndex = localCache.nameIndex.exportPropertyBag()
                '
                Call localCache.guidIndex.getPtr("test")
                localCache.propertyBag_guidIndex = localCache.guidIndex.exportPropertyBag()
                '
                Call cpCore.cache.setKey(cacheName, localCache, cnAddons)
                Call cpCore.cache.setKey("testCache1", localCache, cnAddons)
                Call cpCore.cache.setKey("testCache2", localCache)
                Call cpCore.cache.setKey("testCache3", "sampleName")
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try            '
        End Sub
    End Class
End Namespace