
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
            Public id As Integer
            Public active As Boolean
            Public name As String
            Public ccguid As String
            Public collectionid As Integer
            Public Copy As String
            Public addonCache_ccguid As String
            Public Link As String
            Public ObjectProgramID As String
            Public DotNetClass As String
            Public ArgumentList As String
            Public copyText As String
            Public isInline As Boolean
            Public blockDefaultStyles As Boolean
            Public stylesFilename As String
            Public customStylesFilename As String
            Public formxml As String
            Public remoteAssetLink As String
            Public asAjax As Boolean
            Public InFrame As Boolean
            Public scriptingEntryPoint As String
            Public scriptingLanguageID As Integer
            Public scriptingCode As String
            Public blockEditTools As Boolean
            Public scriptingTimeout As String
            Public inlineScript As String
            Public help As String
            Public helpLink As String
            Public JavaScriptOnLoad As String
            Public JavaScriptBodyEnd As String
            Public PageTitle As String
            Public MetaDescription As String
            Public MetaKeywordList As String
            Public OtherHeadTags As String
            Public JSFilename As String
            Public remoteMethod As Boolean
            Public OnBodyStart As Boolean
            Public OnBodyEnd As Boolean
            Public OnPageStart As Boolean
            Public OnPageEnd As Boolean
            Public robotsTxt As String
        End Class
        '
        <Serializable>
        Public Class addonsCacheClass
            Public addonList As Dictionary(Of String, addonClass)
            Public idIndex As keyPtrController
            Public nameIndex As keyPtrController
            Public guidIndex As keyPtrController
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
        Public addonCache As addonsCacheClass
        '
        '====================================================================================================
        ''' <summary>
        ''' clear the addonCAche
        ''' </summary>
        Public Sub clear()
            Try
                If addonCache Is Nothing Then
                    addonCache = New addonsCacheClass()
                End If
                addonCache.addonList = New Dictionary(Of String, addonClass)
                addonCache.guidIndex = New keyPtrController
                addonCache.idIndex = New keyPtrController
                addonCache.nameIndex = New keyPtrController
                addonCache.onBodyEndPtrs = {}
                addonCache.onBodyStartPtrs = {}
                addonCache.onPageEndPtrs = {}
                addonCache.onPageStartPtrs = {}
                addonCache.propertyBag_guidIndex = ""
                addonCache.propertyBag_idIndex = ""
                addonCache.propertyBag_nameIndex = ""
                Call cpCore.cache.setObject(cacheName, addonCache, cnAddons)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    If addonCache.addonList.Count > 0 Then
                        'hint = hint & ", 3 cache_addonCnt=[" & cache_addons.addonList.Count & "]"
                        For addonPtr = 0 To addonCache.addonList.Count - 1
                            If genericController.EncodeInteger(addonCache.addonList(addonPtr.ToString).id) = RecordID Then
                                Exit For
                            End If
                        Next
                        addonCacheRowOK = False
                        SQL = "select " & cache_addon2_fieldList & " from ccaggregatefunctions where id=" & RecordID
                        addon = New addonClass
                        dt = cpCore.db.executeSql(SQL)
                        If dt.Rows.Count > 0 Then
                            With dt.Rows(0)
                                addon.id = genericController.EncodeInteger(.Item(0))
                                addon.active = genericController.EncodeBoolean(.Item(1))
                                addon.name = genericController.encodeText(.Item(2))
                                addon.ccguid = genericController.encodeText(.Item(3))
                                addon.collectionid = genericController.EncodeInteger(.Item(4))
                                addon.Copy = genericController.encodeText(.Item(5))
                                addon.addonCache_ccguid = genericController.encodeText(.Item(6))
                                addon.Link = genericController.encodeText(.Item(7))
                                addon.ObjectProgramID = genericController.encodeText(.Item(8))
                                addon.DotNetClass = genericController.encodeText(.Item(9))
                                addon.ArgumentList = genericController.encodeText(.Item(10))
                                addon.copyText = genericController.encodeText(.Item(11))
                                addon.isInline = genericController.EncodeBoolean(.Item(12))
                                addon.blockDefaultStyles = genericController.EncodeBoolean(.Item(13))
                                addon.stylesFilename = genericController.encodeText(.Item(14))
                                addon.customStylesFilename = genericController.encodeText(.Item(15))
                                addon.formxml = genericController.encodeText(.Item(16))
                                addon.remoteAssetLink = genericController.encodeText(.Item(17))
                                addon.asAjax = genericController.EncodeBoolean(.Item(18))
                                addon.InFrame = genericController.EncodeBoolean(.Item(19))
                                addon.scriptingEntryPoint = genericController.encodeText(.Item(20))
                                addon.scriptingLanguageID = genericController.EncodeInteger(.Item(21))
                                addon.scriptingCode = genericController.encodeText(.Item(22))
                                addon.blockEditTools = genericController.EncodeBoolean(.Item(23))
                                addon.scriptingTimeout = genericController.encodeText(.Item(24))
                                addon.inlineScript = genericController.encodeText(.Item(25))
                                addon.help = genericController.encodeText(.Item(26))
                                addon.helpLink = genericController.encodeText(.Item(27))
                                addon.JavaScriptOnLoad = genericController.encodeText(.Item(28))
                                addon.JavaScriptBodyEnd = genericController.encodeText(.Item(29))
                                addon.PageTitle = genericController.encodeText(.Item(30))
                                addon.MetaDescription = genericController.encodeText(.Item(31))
                                addon.MetaKeywordList = genericController.encodeText(.Item(32))
                                addon.OtherHeadTags = genericController.encodeText(.Item(33))
                                addon.JSFilename = genericController.encodeText(.Item(34))
                                addon.remoteMethod = genericController.EncodeBoolean(.Item(35))
                                addon.OnBodyStart = genericController.EncodeBoolean(.Item(36))
                                addon.OnBodyEnd = genericController.EncodeBoolean(.Item(37))
                                addon.OnPageStart = genericController.EncodeBoolean(.Item(38))
                                addon.OnPageEnd = genericController.EncodeBoolean(.Item(39))
                                addon.robotsTxt = genericController.encodeText(.Item(40))
                                addon.active = genericController.EncodeBoolean(.Item(41))
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
                            If addonPtr = addonCache.addonList.Count Then
                                '
                                ' not found in cache - add a new entry
                                '
                                'hint = hint & ", 6"
                                addonCache.addonList.Add(addonCache.addonList.Count.ToString, addon)
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
                            Id = addonCache.addonList(addonPtr.ToString).id
                            RecordName = addonCache.addonList(addonPtr.ToString).name
                            '
                            'hint = hint & ", 9"
                            Call addonCache.idIndex.setPtr(genericController.encodeText(Id), addonPtr)
                            '
                            If RecordName <> "" Then
                                'hint = hint & ", 10"
                                Call addonCache.nameIndex.setPtr(RecordName, addonPtr)
                            End If
                            '
                            'hint = hint & ", 11"
                            Call save()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If addonCache.addonList.Count <= 0 Then
                    '
                ElseIf (addonCache.idIndex Is Nothing) Then
                    '
                Else
                    If genericController.vbIsNumeric(addonNameGuidOrId) Then
                        ReturnPtr = addonCache.idIndex.getPtr(addonNameGuidOrId.ToUpper())
                    End If
                    If ReturnPtr < 0 Then
                        ReturnPtr = addonCache.nameIndex.getPtr(addonNameGuidOrId)
                        If ReturnPtr < 0 Then
                            ReturnPtr = addonCache.guidIndex.getPtr(CStr(addonNameGuidOrId))
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
                            If addonCache.addonList.Count <= 0 Then
                                '
                            ElseIf (addonCache.idIndex Is Nothing) Then
                                '
                            Else
                                ReturnPtr = addonCache.idIndex.getPtr(CStr(addonId))
                            End If
                        End If
                    End If
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If addonCache Is Nothing Then
                    needLoad = True
                ElseIf addonCache.addonList Is Nothing Then
                    needLoad = True
                ElseIf addonCache.addonList.Count = 0 Then
                    needLoad = True
                End If
                If needLoad Then
                    '
                    ' Load cached addonCache
                    '
                    Try
                        cacheTest = cpCore.cache.getObject(Of addonsCacheClass)(cacheName)
                        'Dim testCache1 = cpCore.cache.getObject(Of addonsCacheClass)("testCache1")
                        'Dim testCache2 = cpCore.cache.getObject(Of addonsCacheClass)("testCache2")
                        'Dim testCache3 = cpCore.cache.getObject(Of String)("testCache3")
                        If Not IsNothing(cacheTest) Then
                            addonCache = DirectCast(cacheTest, addonsCacheClass)
                            If Not IsNothing(addonCache) Then
                                If addonCache.addonList.Count > 0 Then
                                    addonCache.idIndex.importPropertyBag(addonCache.propertyBag_idIndex)
                                    addonCache.nameIndex.importPropertyBag(addonCache.propertyBag_nameIndex)
                                    addonCache.guidIndex.importPropertyBag(addonCache.propertyBag_guidIndex)
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
                        addonCache = New addonsCacheClass
                        addonCache.addonList = New Dictionary(Of String, addonClass)
                        addonCache.guidIndex = New keyPtrController
                        addonCache.idIndex = New keyPtrController
                        addonCache.nameIndex = New keyPtrController
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
                                    addon.active = genericController.EncodeBoolean(dr.Item("active"))
                                    addon.ArgumentList = genericController.encodeText(dr.Item("ArgumentList"))
                                    addon.asAjax = genericController.EncodeBoolean(dr.Item("AsAjax"))
                                    addon.blockDefaultStyles = genericController.EncodeBoolean(dr.Item("BlockDefaultStyles"))
                                    addon.blockEditTools = genericController.EncodeBoolean(dr.Item("BlockEditTools"))
                                    addon.addonCache_ccguid = genericController.encodeText(dr.Item("ccguid"))
                                    addon.collectionid = genericController.EncodeInteger(dr.Item("collectionid"))
                                    addon.Copy = genericController.encodeText(dr.Item("Copy"))
                                    addon.copyText = genericController.encodeText(dr.Item("CopyText"))
                                    addon.customStylesFilename = genericController.encodeText(dr.Item("CustomStylesFilename"))
                                    addon.DotNetClass = genericController.encodeText(dr.Item("DotNetClass"))
                                    addon.formxml = genericController.encodeText(dr.Item("formxml"))
                                    addon.ccguid = RecordGuid
                                    addon.help = genericController.encodeText(dr.Item("help"))
                                    addon.helpLink = genericController.encodeText(dr.Item("helpLink"))
                                    addon.id = RecordID
                                    addon.InFrame = genericController.EncodeBoolean(dr.Item("InFrame"))
                                    addon.inlineScript = genericController.encodeText(dr.Item("inlineScript"))
                                    addon.isInline = genericController.EncodeBoolean(dr.Item("IsInline"))
                                    addon.JavaScriptBodyEnd = genericController.encodeText(dr.Item("JavaScriptBodyEnd"))
                                    addon.JavaScriptOnLoad = genericController.encodeText(dr.Item("JavaScriptOnLoad"))
                                    addon.JSFilename = genericController.encodeText(dr.Item("JSFilename"))
                                    addon.Link = genericController.encodeText(dr.Item("Link"))
                                    addon.MetaDescription = genericController.encodeText(dr.Item("MetaDescription"))
                                    addon.MetaKeywordList = genericController.encodeText(dr.Item("MetaKeywordList"))
                                    addon.name = RecordName
                                    addon.ObjectProgramID = genericController.encodeText(dr.Item("ObjectProgramID"))
                                    addon.OnBodyEnd = onBodyEnd
                                    addon.OnBodyStart = onBodyStart
                                    addon.OnPageEnd = onPageEnd
                                    addon.OnPageStart = onPageStart
                                    addon.OtherHeadTags = genericController.encodeText(dr.Item("OtherHeadTags"))
                                    addon.PageTitle = genericController.encodeText(dr.Item("PageTitle"))
                                    addon.remoteAssetLink = genericController.encodeText(dr.Item("RemoteAssetLink"))
                                    addon.remoteMethod = genericController.EncodeBoolean(dr.Item("remoteMethod"))
                                    addon.robotsTxt = robotsTxt
                                    addon.scriptingCode = genericController.encodeText(dr.Item("ScriptingCode"))
                                    addon.scriptingEntryPoint = genericController.encodeText(dr.Item("ScriptingEntryPoint"))
                                    addon.scriptingLanguageID = genericController.EncodeInteger(dr.Item("ScriptingLanguageID"))
                                    addon.scriptingTimeout = genericController.encodeText(dr.Item("ScriptingTimeout"))
                                    addon.stylesFilename = genericController.encodeText(dr.Item("StylesFilename"))
                                    addonCache.addonList.Add(addonPtr.ToString, addon)
                                    '
                                    'hint = hint & ",1"
                                    Call addonCache.idIndex.setPtr(genericController.encodeText(RecordID), addonPtr)
                                    If RecordName <> "" Then
                                        Call addonCache.nameIndex.setPtr(RecordName, addonPtr)
                                    End If
                                    If RecordGuid <> "" Then
                                        Call addonCache.guidIndex.setPtr(RecordGuid, addonPtr)
                                    End If
                                    If onBodyStart Then
                                        ' save Id for now, lookup before save
                                        ReDim Preserve addonCache.onBodyStartPtrs(onBodyStartCnt)
                                        addonCache.onBodyStartPtrs(onBodyStartCnt) = RecordID
                                        onBodyStartCnt = onBodyStartCnt + 1
                                    End If
                                    If onBodyEnd Then
                                        ReDim Preserve addonCache.onBodyEndPtrs(onBodyEndCnt)
                                        addonCache.onBodyEndPtrs(onBodyEndCnt) = RecordID
                                        onBodyEndCnt = onBodyEndCnt + 1
                                    End If
                                    If onPageStart Then
                                        ReDim Preserve addonCache.onPageStartPtrs(onPageStartCnt)
                                        addonCache.onPageStartPtrs(onPageStartCnt) = RecordID
                                        onPageStartCnt = onPageStartCnt + 1
                                    End If
                                    'hint = hint & ",2"
                                    If onPageEnd Then
                                        ReDim Preserve addonCache.onPageEndPtrs(onPageEndCnt)
                                        addonCache.onPageEndPtrs(onPageEndCnt) = RecordID
                                        onPageEndCnt = onPageEndCnt + 1
                                    End If
                                    'hint = hint & ",3"
                                    'robotsTxt = genericController.encodeText(cache_addons.addonList(addonPtr).addoncache_robotsTxt)
                                    'hint = hint & ",4"
                                    If robotsTxt <> "" Then
                                        'hint = hint & ",5"
                                        addonCache.robotsTxt = addonCache.robotsTxt & vbCrLf & robotsTxt
                                    End If
                                    'hint = hint & ",6"
                                    addonPtr += 1
                                Next
                            End If
                        End Using
                        If addonCache.addonList.Count > 0 Then
                            save()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call addonCache.idIndex.getPtr("test")
                addonCache.propertyBag_idIndex = addonCache.idIndex.exportPropertyBag()
                '
                Call addonCache.nameIndex.getPtr("test")
                addonCache.propertyBag_nameIndex = addonCache.nameIndex.exportPropertyBag()
                '
                Call addonCache.guidIndex.getPtr("test")
                addonCache.propertyBag_guidIndex = addonCache.guidIndex.exportPropertyBag()
                '
                Call cpCore.cache.setObject(cacheName, addonCache, cnAddons)
                'Call cpCore.cache.setObject("testCache1", localCache, cnAddons)
                'Call cpCore.cache.setObject("testCache2", localCache)
                'Call cpCore.cache.setObject("testCache3", "sampleName")
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try            '
        End Sub
    End Class
End Namespace