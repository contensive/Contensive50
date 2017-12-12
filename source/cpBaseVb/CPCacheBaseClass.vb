'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    ''' <summary>
    ''' CP.Cache - contains features to perform simple caching functions
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CPCacheBaseClass
        '
        ''' <summary>
        ''' clear all cache based on any content in a list provided
        ''' </summary>
        ''' <param name="ContentNameList"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Clear(ByVal ContentNameList As String)
        ''' <summary>
        ''' Read the value of a cache. If the cache is cleared, an empty string is returned.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function Read(ByVal key As String) As String
        Public MustOverride Function getObject(ByVal key As String) As Object
        Public MustOverride Function getText(ByVal key As String) As String
        Public MustOverride Function getInteger(ByVal key As String) As Integer
        Public MustOverride Function getNumber(ByVal key As String) As Double
        Public MustOverride Function getDate(ByVal key As String) As Date
        Public MustOverride Function getBoolean(ByVal key As String) As Boolean
        Public MustOverride Sub Save(ByVal key As String, ByVal Value As String)
        Public MustOverride Sub Save(ByVal key As String, ByVal Value As String, ByVal tagCommaList As String)
        Public MustOverride Sub Save(ByVal key As String, ByVal Value As String, ByVal tagCommaList As String, ByVal ClearOnDate As Date)
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object)
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object, ByVal invalidationDate As Date)
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object, ByVal tagList As List(Of String))
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object, ByVal invalidationDate As Date, ByVal tagList As List(Of String))
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object, ByVal tag As String)
        Public MustOverride Sub setKey(ByVal key As String, ByVal Value As Object, ByVal invalidationDate As Date, ByVal tag As String)
        ''' <summary>
        ''' Clear all system caches. Use this call to flush all internal caches.
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub ClearAll()
        '
        Public MustOverride Sub InvalidateTag(ByVal tag As String)
        Public MustOverride Sub InvalidateAll()
        Public MustOverride Sub InvalidateTagList(ByVal tagList As List(Of String))
        Public MustOverride Sub InvalidateContentRecord(ByVal contentName As String, recordId As Integer)
    End Class

End Namespace

