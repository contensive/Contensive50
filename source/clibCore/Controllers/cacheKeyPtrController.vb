
Option Explicit On
Option Strict On



Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' Creates a simple keyValue cache system
    ''' not IDisposable - not contained classes that need to be disposed
    ''' constructor includes query to load with two fields, key (string) and value (string)
    ''' values are stored in a List, referenced by a pointer
    ''' keys are stored in an keyPtrINdex with the pointer to the value
    ''' loads with cache. if cache is empty, it loads on demand
    ''' when cleared, loads on demand
    '''  combines a key ptr index and a value store, referenced by ptr
    ''' </summary>
    Public Class cacheKeyPtrController
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        ' ----- private globals
        '
        Private sqlLoadKeyValue As String
        Private cacheName As String
        Private cacheInvalidationTagCommaList As String
        '
        '====================================================================================================
        '
        Public Class dataStoreClass
            Public dataList As List(Of String)
            Public keyPtrIndex As keyPtrController
            Public loaded As Boolean
        End Class
        Private dataStore As New dataStoreClass
        '
        '====================================================================================================
        '
        Public Sub New(cpCore As coreClass, cacheName As String, sqlLoadKeyValue As String, cacheInvalidationTagCommaList As String)
            MyBase.New()
            Me.cpCore = cpCore
            Me.cacheName = cacheName
            Me.sqlLoadKeyValue = sqlLoadKeyValue
            Me.cacheInvalidationTagCommaList = cacheInvalidationTagCommaList
            dataStore = New dataStoreClass
            dataStore.dataList = New List(Of String)
            dataStore.keyPtrIndex = New keyPtrController
            dataStore.loaded = False
        End Sub
        '
        '====================================================================================================
        '
        '   clear sharedStylesAddonRules cache
        '
        Public Sub clear()
            Try
                dataStore.loaded = False
                dataStore.dataList.Clear()
                dataStore.keyPtrIndex = New keyPtrController
                Call cpCore.cache.setObject(cacheName & "-dataList", dataStore.dataList)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get an integer pointer to the array that contains the keys value. Returns -1 if not found
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function getFirstPtr(key As String) As Integer
            Dim returnPtr As Integer = -1
            Try
                If String.IsNullOrEmpty(key) Then
                    Throw New ArgumentException("blank key is not valid.")
                Else
                    If Not dataStore.loaded Then
                        Call load()
                    End If
                    If Not dataStore.loaded Then
                        Throw New ApplicationException("datastore could not be loaded")
                    Else
                        returnPtr = dataStore.keyPtrIndex.getPtr(key)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex, "key[" & key & "]")
            End Try
            Return returnPtr
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns the next pointer in the array, sorted by value. Returns -1 if there are no more values
        ''' </summary>
        ''' <returns></returns>
        Public Function getNextPtr() As Integer
            Dim returnPtr As Integer = -1
            Try
                If Not dataStore.loaded Then
                    Call load()
                End If
                If Not dataStore.loaded Then
                    Throw New ApplicationException("datastore could not be loaded")
                Else
                    returnPtr = dataStore.keyPtrIndex.getNextPtr()
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnPtr
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return an integer pointer to the value for this key. -1 if not found
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function getPtr(key As String) As Integer
            Dim returnPtr As Integer = -1
            Try
                If String.IsNullOrEmpty(key) Then
                    Throw New ArgumentException("blank key is not valid.")
                Else
                    If Not dataStore.loaded Then
                        Call load()
                    End If
                    If Not dataStore.loaded Then
                        Throw New ApplicationException("datastore could not be loaded")
                    Else
                        returnPtr = dataStore.keyPtrIndex.getPtr(key)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex, "key[" & key & "]")
            End Try
            Return returnPtr
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns the value stored with this pointer.
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Function getValue(ptr As Integer) As String
            Dim returnValue As String = ""
            Try
                If (ptr < 0) Then
                    Throw New ArgumentException("ptr must be >= 0")
                Else
                    If Not dataStore.loaded Then
                        Call load()
                    End If
                    If Not dataStore.loaded Then
                        Throw New ApplicationException("datastore could not be loaded")
                    Else
                        returnValue = dataStore.dataList(ptr)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex, "ptr[" & ptr.ToString() & "]")
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' load the values based on the sql statement loaded during constructor sqlLoadKeyVAlue.
        ''' If already loaded, does nothing
        ''' Attempts to read the cache from the cache, invalidated by the contentname
        ''' </summary>
        Private Sub load()
            Try
                '
                Dim Ptr As Integer
                Dim RecordIdTextValue As String
                Dim needsToReload As Boolean
                '
                If Not dataStore.loaded Then
                    Try
                        needsToReload = True
                        dataStore = DirectCast(cpCore.cache.getObject(Of dataStoreClass)(cacheName), dataStoreClass)
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
                        dataStore.keyPtrIndex = New keyPtrController
                        Using dt As DataTable = cpCore.db.executeSql(sqlLoadKeyValue)
                            Ptr = 0
                            For Each dr As DataRow In dt.Rows
                                dataStore.keyPtrIndex.setPtr(dr.Item(0).ToString, Ptr)
                                dataStore.dataList.Add(dr.Item(1).ToString)
                                Ptr += 1
                            Next
                        End Using
                        '
                        If dataStore.dataList.Count > 0 Then
                            dataStore.keyPtrIndex = New keyPtrController
                            For Ptr = 0 To dataStore.dataList.Count - 1
                                RecordIdTextValue = dataStore.dataList(Ptr)
                                Call dataStore.keyPtrIndex.setPtr(RecordIdTextValue, Ptr)
                            Next
                            Call updateCache()
                        End If
                        needsToReload = False
                    End If
                    dataStore.loaded = True
                End If
            Catch ex As Exception
                Throw New ApplicationException("Exception in cacheKeyPtrClass.load", ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        Public Sub updateCache()
            Try
                If dataStore.loaded Then
                    Call dataStore.keyPtrIndex.getPtr("test")
                    Call cpCore.cache.setObject(cacheName, dataStore, cacheInvalidationTagCommaList)
                End If
            Catch ex As Exception
                Throw New ApplicationException("Exception in cacheKeyPtrClass.save", ex)
            End Try
        End Sub
    End Class

End Namespace