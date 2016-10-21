'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    ''' <summary>
    ''' CP.Block - an object that holds and manipulates a block of html
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CPBlockBaseClass
        ''' <summary>
        ''' Load the block with an html string
        ''' </summary>
        ''' <param name="htmlString"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Load(ByVal htmlString As String)
        ''' <summary>
        ''' load the block with the entire contents of a file in the wwwRoot
        ''' </summary>
        ''' <param name="wwwFileName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub OpenFile(ByVal wwwFileName As String)
        ''' <summary>
        ''' load the block with the contents of a record in Copy Content
        ''' </summary>
        ''' <param name="copyRecordName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub OpenCopy(ByVal copyRecordName As String)
        ''' <summary>
        ''' load the block with the contents of a record in Layouts
        ''' </summary>
        ''' <param name="layoutRecordName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub OpenLayout(ByVal layoutRecordName As String)
        ''' <summary>
        ''' load the block with the body of a file in the wwwRoot
        ''' </summary>
        ''' <param name="wwwFileName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub ImportFile(ByVal wwwFileName As String)
        ''' <summary>
        ''' set the innerHtml of an element in the current block specified by the findSelector
        ''' </summary>
        ''' <param name="findSelector"></param>
        ''' <param name="htmlString"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetInner(ByVal findSelector As String, ByVal htmlString As String)
        ''' <summary>
        ''' Return the innerHtml from the current block specified by the findSelector
        ''' </summary>
        ''' <param name="findSelector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetInner(ByVal findSelector As String) As String
        ''' <summary>
        ''' Set the OuterHtml in the current block specified by the findSelector to the htmlString
        ''' </summary>
        ''' <param name="findSelector"></param>
        ''' <param name="htmlString"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetOuter(ByVal findSelector As String, ByVal htmlString As String)
        ''' <summary>
        ''' return the outer Html specified by the findSelector
        ''' </summary>
        ''' <param name="findSelector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetOuter(ByVal findSelector As String) As String
        ''' <summary>
        ''' append the htmlString into the current Block
        ''' </summary>
        ''' <param name="htmlString"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Append(ByVal htmlString As String)
        ''' <summary>
        ''' Prepend the htmlString on the current block
        ''' </summary>
        ''' <param name="htmlString"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Prepend(ByVal htmlString As String)
        ''' <summary>
        ''' return the entire html of the current block
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetHtml() As String
        ''' <summary>
        '''  Clear the contents of the current block
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub Clear()
    End Class
End Namespace


