Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    Public Class CPCacheClass
        Inherits BaseClasses.CPCacheBaseClass
        Implements IDisposable
        '
        Private cpCore As Contensive.Core.coreClass
        Private cp As CPClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpParent"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Clear all cache values
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub ClearAll()
            Call cpCore.cache.invalidateAll()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' clear cacheDataSourceTag. A cache DataSource Tag is a tag that represents a source of data used to build a cache object, like a database table.
        ''' </summary>
        ''' <param name="ContentNameList"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Clear(ByVal cacheDataSourceTag As String)
            Call cpCore.cache.invalidateTagCommaList(cacheDataSourceTag)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' read a cache value
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Read(ByVal Name As String) As String
            Return getText(Name)
        End Function
        '====================================================================================================
        ''' <summary>
        ''' save a cache value. Legacy. Use object value.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="Value"></param>
        ''' <param name="invalidationTagCommaList"></param>
        ''' <param name="ClearOnDate"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Save(ByVal key As String, ByVal Value As String, Optional ByVal invalidationTagCommaList As String = "", Optional ByVal invalidationDate As Date = #12:00:00 AM#)
            Try
                Dim invalidationTagList As New List(Of String)
                If String.IsNullOrEmpty(invalidationTagCommaList.Trim) Then
                    '
                Else
                    invalidationTagList.AddRange(invalidationTagCommaList.Split(","))
                End If
                If (invalidationDate = #12:00:00 AM#) Then
                    Call cpCore.cache.setKey(key, Value, invalidationTagList)
                Else
                    Call cpCore.cache.setKey(key, Value, invalidationDate, invalidationTagList)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function getObject(key As String) As Object
            Return cpCore.cache.getObject(Of Object)(key)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function getInteger(key As String) As Integer
            Return EncodeInteger(getObject(key))
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function getBoolean(key As String) As Boolean
            Return EncodeBoolean(getObject(key))
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function getDate(key As String) As Date
            Return EncodeDate(getObject(key))
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function getNumber(key As String) As Double
            Return EncodeNumber(getObject(key))
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function getText(key As String) As String
            Return EncodeText(getObject(key))
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub InvalidateAll()
            cpCore.cache.invalidateAll()
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub InvalidateTag(tag As String)
            cpCore.cache.invalidateTag(tag)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub InvalidateTagList(tagList As List(Of String))
            cpCore.cache.invalidateTagList(tagList)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Save a value to a cache key. It will invalidate after the default invalidation days
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        Public Overrides Sub SetKey(key As String, value As Object)
            cpCore.cache.setKey(key, value)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Save a value to a cache key and specify when it will be invalidated.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="invalidationDate"></param>
        Public Overrides Sub SetKey(key As String, value As Object, invalidationDate As Date)
            cpCore.cache.setKey(key, value, invalidationDate)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Save a value to a cachekey and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="tagList"></param>
        Public Overrides Sub SetKey(key As String, value As Object, tagList As List(Of String))
            cpCore.cache.setKey(key, value, tagList)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Save a value to a cachekey with an invalidate date, and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="tagList"></param>
        ''' <param name="invalidationDate"></param>
        Public Overrides Sub SetKey(key As String, value As Object, invalidationDate As Date, tagList As List(Of String))
            cpCore.cache.setKey(key, value, invalidationDate, tagList)
        End Sub
        '
        Public Overrides Sub setKey(key As String, Value As Object, tag As String)
            cpCore.cache.setKey(key, Value, tag)
        End Sub
        '
        Public Overrides Sub setKey(key As String, Value As Object, invalidationDate As Date, tag As String)
            cpCore.cache.setKey(key, Value, invalidationDate, tag)
        End Sub
#Region " IDisposable Support "
        '
        ' dispose
        '
        Protected disposed As Boolean = False
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cp = Nothing
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace