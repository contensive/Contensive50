Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPCacheClass.ClassId, CPCacheClass.InterfaceId, CPCacheClass.EventsId)> _
    Public Class CPCacheClass
        Inherits BaseClasses.CPCacheBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "D522F0F5-53DF-4C6C-88E5-75CDAB91D286"
        Public Const InterfaceId As String = "9FED1031-1637-4002-9B08-4A40FDF13236"
        Public Const EventsId As String = "11B23802-CBD3-48E6-9C3E-1DC26ED8775A"
#End Region
        '
        Private cpCore As Contensive.Core.cpCoreClass
        Private cp As CPClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpParent"></param>
        ''' <remarks></remarks>
        Friend Sub New(ByRef cpParent As CPClass)
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
            Call cpCore.app.cache_invalidateAll()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' clear cacheDataSourceTag. A cache DataSource Tag is a tag that represents a source of data used to build a cache object, like a database table.
        ''' </summary>
        ''' <param name="ContentNameList"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Clear(ByVal cacheDataSourceTag As String)
            Call cpCore.app.cache_invalidateTagList(cacheDataSourceTag)
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
            Dim returnString As String = ""
            Try
                returnString = EncodeText(cpCore.app.cache_read(Of String)(Name))
            Catch ex As Exception
                cp.core.handleException(ex, "Unexpected error in cp.cache.read()")
                returnString = ""
            End Try
            Return returnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <param name="ClearOnContentChangeList"></param>
        ''' <param name="ClearOnDate"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Save(ByVal Name As String, ByVal Value As String, Optional ByVal ClearOnContentChangeList As String = "", Optional ByVal invalidationDate As Date = #12:00:00 AM#)
            invalidationDate = encodeDateMinValue(invalidationDate)
            Call cpCore.app.cache_save(Name, Value, ClearOnContentChangeList, invalidationDate)
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