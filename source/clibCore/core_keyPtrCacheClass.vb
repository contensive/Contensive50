

Namespace Contensive.Core
    Public Class keyPtrCacheClass
        '
        ' combines a key ptr index and a value store, referenced by ptr
        '
        ' constructor includes query to load with two fields, key (string) and value (string)
        ' values are stored in a List, referenced by a pointer
        ' keys are stored in an keyPtrINdex with the pointer to the value
        ' loads with cache. if cache is empty, it loads on demand
        ' when cleared, loads on demand
        '
        Private app As appServicesClass
        Private cacheName As String
        '
        Public sqlLoad As String
        '
        Public Class dataStoreClass
            Public dataList As List(Of String)
            Public keyPtrIndex As keyPtrIndexClass
            Public loaded As Boolean
        End Class
        Private dataStore As New dataStoreClass
        '
        Public Sub New(app As appServicesClass, cacheName As String, sqlLoadKeyValue As String)
            Me.app = app
            Me.cacheName = cacheName
            Me.sqlLoad = sqlLoadKeyValue
            dataStore = New dataStoreClass
            dataStore.dataList = New List(Of String)
            dataStore.keyPtrIndex = New keyPtrIndexClass
            dataStore.loaded = False
        End Sub
        '
        '   clear sharedStylesAddonRules cache
        '
        Public Sub clear()
            Try
                dataStore.loaded = False
                dataStore.dataList.Clear()
                dataStore.keyPtrIndex = New keyPtrIndexClass
                Call app.cache_saveRaw(cacheName & "-dataList", dataStore.dataList)
            Catch ex As Exception

            End Try
        End Sub
        '
        '   getPtr sharedStylesAddonRules cache
        '
        Friend Function getFirstPtr(key As String) As Integer
            Dim returnPtr As Integer = -1
            Try
                If Not dataStore.loaded Then
                    Call load()
                End If
                If dataStore.loaded Then
                    returnPtr = dataStore.keyPtrIndex.getPtr(key)
                End If
            Catch ex As Exception

            End Try
            Return returnPtr
        End Function
        '
        Friend Function getNextPtr() As Integer
            Dim returnPtr As Integer = -1
            Try
                If Not dataStore.loaded Then
                    Call load()
                End If
                If dataStore.loaded Then
                    returnPtr = dataStore.keyPtrIndex.getNextPtr()
                End If
            Catch ex As Exception

            End Try
            Return returnPtr
        End Function
        '
        Public Function getPtr(key As String) As Integer
            Try
                If Not dataStore.loaded Then
                    Call load()
                End If
                If dataStore.loaded Then
                    Dim ptr As Integer = dataStore.keyPtrIndex.getPtr(key)
                    Return dataStore.dataList(ptr)
                End If

            Catch ex As Exception

            End Try

        End Function
        '
        Public Function getValue(ptr As Integer) As String
            Try
                If Not dataStore.loaded Then
                    Call load()
                End If
                If dataStore.loaded Then
                    Return dataStore.dataList(ptr)
                End If
            Catch ex As Exception

            End Try

        End Function
        '
        '   load sharedStylesAddonRules cache
        '
        Private Sub load()
            Try
                '
                Dim Ptr As Integer
                Dim RecordAddonID As Integer
                Dim needsToReload As Boolean
                '
                If Not dataStore.loaded Then
                    Try
                        needsToReload = True
                        dataStore = DirectCast(app.cache_readRaw(cacheName), dataStoreClass)
                    Catch ex As Exception
                        needsToReload = True
                    End Try
                    If (dataStore Is Nothing) Then
                        needsToReload = True
                    ElseIf (dataStore.dataList Is Nothing) Then
                        needsToReload = True
                    End If
                    If needsToReload Then
                        '
                        ' cache is empty, build it from scratch
                        '
                        dataStore = New dataStoreClass
                        dataStore.dataList = New List(Of String)
                        dataStore.keyPtrIndex = New keyPtrIndexClass
                        Using dt As DataTable = app.executeSql(sqlLoad)
                            Ptr = 0
                            For Each dr As DataRow In dt.Rows
                                dataStore.keyPtrIndex.setPtr(dr.Item(0).ToString, Ptr)
                                dataStore.dataList.Add(dr.Item(1).ToString)
                                Ptr += 1
                            Next
                        End Using
                        '
                        If dataStore.dataList.Count > 0 Then
                            dataStore.keyPtrIndex = New keyPtrIndexClass
                            For Ptr = 0 To dataStore.dataList.Count - 1
                                RecordAddonID = dataStore.dataList(Ptr)
                                Call dataStore.keyPtrIndex.setPtr(EncodeText(RecordAddonID), Ptr)
                            Next
                            Call save()
                        End If
                        needsToReload = False
                    End If
                    dataStore.loaded = True
                End If
                '
            Catch ex As Exception

            End Try
        End Sub
        '
        '
        '
        Friend Sub save()
            Try
                If dataStore.loaded Then '
                    Call dataStore.keyPtrIndex.getPtr("test")
                    Call app.cache_saveRaw(cacheName, dataStore)
                End If
            Catch ex As Exception

            End Try
        End Sub
    End Class
End Namespace