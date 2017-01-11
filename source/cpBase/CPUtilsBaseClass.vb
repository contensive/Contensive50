'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPUtilsBaseClass
        Enum addonContext
            ContextPage = 1
            ContextAdmin = 2
            ContextTemplate = 3
            ContextEmail = 4
            ContextRemoteMethodHtml = 5
            ContextOnNewVisit = 6
            ContextOnPageEnd = 7
            ContextOnPageStart = 8
            ContextEditor = 9
            ContextHelpUser = 10
            ContextHelpAdmin = 11
            ContextHelpDeveloper = 12
            ContextOnContentChange = 13
            ContextFilter = 14
            ContextSimple = 15
            ContextOnBodyStart = 16
            ContextOnBodyEnd = 17
            ContextRemoteMethodJson = 18
        End Enum

        Public MustOverride Function ConvertHTML2Text(ByVal Source As String) As String
        Public MustOverride Function ConvertText2HTML(ByVal Source As String) As String
        Public MustOverride Function CreateGuid() As String
        Public MustOverride Function DecodeUrl(ByVal Url As String) As String
        Public MustOverride Function EncodeContentForWeb(ByVal Source As String, Optional ByVal ContextContentName As String = "", Optional ByVal ContextRecordID As Integer = 0, Optional ByVal WrapperID As Integer = 0) As String
        Public MustOverride Function EncodeHtmlForWysiwygEditor(ByVal Source As String) As String
        Public MustOverride Function DecodeHtmlFromWysiwygEditor(ByVal Source As String) As String
        Public MustOverride Function DecodeHTML(ByVal Source As String) As String
        Public MustOverride Function EncodeHTML(ByVal Source As String) As String
        Public MustOverride Function EncodeUrl(ByVal Source As String) As String
        Public MustOverride Function GetPleaseWaitEnd() As String
        Public MustOverride Function GetPleaseWaitStart() As String
        Public MustOverride Sub IISReset()
        Public MustOverride Function EncodeInteger(ByVal Expression As Object) As Integer
        Public MustOverride Function EncodeNumber(ByVal Expression As Object) As Double
        Public MustOverride Function EncodeText(ByVal Expression As Object) As String
        Public MustOverride Function EncodeBoolean(ByVal Expression As Object) As Boolean
        Public MustOverride Function EncodeDate(ByVal Expression As Object) As Date
        Public MustOverride Function ExecuteAddon(ByVal IdGuidOrName As String) As String
        Public MustOverride Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal WrapperId As Integer) As String
        Public MustOverride Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal context As addonContext) As String
        Public MustOverride Function ExecuteAddonAsProcess(ByVal IdGuidOrName As String) As String
        Public MustOverride Sub AppendLog(ByVal pathFilename As String, ByVal logText As String)
        Public MustOverride Sub AppendLog(ByVal logText As String)
        Public MustOverride Function ConvertLinkToShortLink(ByVal URL As String, ByVal ServerHost As String, ByVal ServerVirtualPath As String) As String
        Public MustOverride Function ConvertShortLinkToLink(ByVal URL As String, ByVal PathPagePrefix As String) As String
        Public MustOverride Function DecodeGMTDate(ByVal GMTDate As String) As Date
        Public MustOverride Function DecodeResponseVariable(ByVal Source As String) As String
        Public MustOverride Function EncodeJavascript(ByVal Source As String) As String
        Public MustOverride Function EncodeQueryString(ByVal Source As String) As String
        Public MustOverride Function EncodeRequestVariable(ByVal Source As String) As String
        Public MustOverride Function GetArgument(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "", Optional ByVal Delimiter As String = "") As String
        Public MustOverride Function GetFilename(ByVal PathFilename As String) As String
        Public MustOverride Function GetFirstNonZeroDate(ByVal Date0 As Date, ByVal Date1 As Date) As Date
        Public MustOverride Function GetFirstNonZeroInteger(ByVal Integer0 As Integer, ByVal Integer1 As Integer) As Integer
        Public MustOverride Function GetIntegerString(ByVal Value As Integer, ByVal DigitCount As Integer) As String
        Public MustOverride Function GetLine(ByVal Body As String) As String
        Public MustOverride Function GetListIndex(ByVal Item As String, ByVal ListOfItems As String) As Integer
        Public MustOverride Function GetProcessID() As Integer
        Public MustOverride Function GetRandomInteger() As Integer
        Public MustOverride Function IsInDelimitedString(ByVal DelimitedString As String, ByVal TestString As String, ByVal Delimiter As String) As Boolean
        Public MustOverride Function ModifyLinkQueryString(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
        Public MustOverride Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
        Public MustOverride Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        Public MustOverride Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        Public MustOverride Function SplitDelimited(ByVal WordList As String, ByVal Delimiter As String) As Object
        Public MustOverride Sub Sleep(ByVal timeMSec As Integer)
        Public MustOverride Function hashMd5(ByVal source As String) As String
        Public MustOverride Function isGuid(ByVal guid As String) As Boolean
        Public MustOverride Sub Upgrade(ByVal isNewApp As Boolean)
        Public MustOverride Function installCollectionFromFile(privateFile As String) As Integer ' returns taskId
        Public MustOverride Function installCollectionFromLibrary(collectionGuid As String) As Integer ' returns taskId
        Public MustOverride Function installCollectionFromLink(link As String) As Integer ' returns taskId
        'Public MustOverride Function getTaskStatus(taskId As Integer) As Integer ' returns status codes
    End Class
End Namespace

