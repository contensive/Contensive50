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
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function Read(ByVal Name As String) As String
        ''' <summary>
        ''' Save a string to a name. If a change is made to any of the content is the given list or if the clearbydate is passed, the cache is cleared.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <param name="ClearOnContentChangeList"></param>
        ''' <param name="ClearOnDate"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Save(ByVal Name As String, ByVal Value As String, Optional ByVal ClearOnContentChangeList As String = "", Optional ByVal ClearOnDate As Date = #12:00:00 AM#) 'Implements BaseClasses.CPCacheBaseClass.Save
        ''' <summary>
        ''' Clear all system caches. Use this call to flush all internal caches.
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub ClearAll()
    End Class

End Namespace

