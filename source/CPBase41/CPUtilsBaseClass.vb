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
        '
        Public Class addonExecuteHostRecordContext
            Public contentName As String
            Public recordId As Integer
            Public fieldName As String
        End Class
        '
        Public Class addonExecuteContext
            ''' <summary>
            ''' This caption is used if the addon cannot be executed.
            ''' </summary>
            ''' <returns></returns>
            Public Property errorCaption As String
            ''' <summary>
            ''' select enumeration option the describes the environment in which the addon is being executed (in an email, on a page, as a remote method, etc)
            ''' </summary>
            ''' <returns></returns>
            Public Property addonType As addonContext = addonContext.ContextSimple
            ''' <summary>
            ''' Optional. If the addon is run from a page, it includes an instanceGuid which can be used by addon programming to locate date for this instance.
            ''' </summary>
            ''' <returns></returns>
            Public Property instanceGuid As String = ""
            ''' <summary>
            ''' Optional. Name value pairs added to the document environment during execution so they be read by addon programming during and after execution with cp.doc.getText(), etc.
            ''' </summary>
            ''' <returns></returns>
            Public Property instanceArguments As New Dictionary(Of String, String)
            ''' <summary>
            ''' Optional. If this addon is run automatically because it was included in content, this is the contentName, recordId and fieldName of the record that held that content.
            ''' </summary>
            ''' <returns></returns>
            Public Property hostRecord As New addonExecuteHostRecordContext()
            ''' <summary>
            ''' Optional. If included, this is the id value of a record in the Wrappers content and that wrapper will be added to the addon return result.
            ''' </summary>
            ''' <returns></returns>
            Public Property wrapperID As Integer = 0
            ''' <summary>
            ''' Optional. If included, the addon will be wrapped with a div and this will be the html Id value of the div. May be used to customize the resulting html styles.
            ''' </summary>
            ''' <returns></returns>
            Public Property cssContainerId As String = ""
            ''' <summary>
            ''' Optional. If included, the addon will be wrapped with a div and this will be the html class value of the div. May be used to customize the resulting html styles.
            ''' </summary>
            ''' <returns></returns>
            Public Property cssContainerClass As String = ""
            ''' <summary>
            ''' Optional. If included with personizationPeopleId, the addon will be run in a authentication context for this people record. If not included, the current documents authentication context is used. This may be used for cases like addons that send email where email content may include personalization.
            ''' </summary>
            ''' <returns></returns>
            Public Property personalizationPeopleId As Integer = 0
            ''' <summary>
            ''' Optional. If included with personizationPeopleId, the addon will be run in a authentication context for this people record. If not included, the current documents authentication context is used. This may be used for cases like addons that send email where email content may include personalization.
            ''' </summary>
            ''' <returns></returns>
            Public Property personalizationAuthenticated As Boolean = False
            ''' <summary>
            ''' Optional. If true, this addon is called because it was a dependancy, and can only be called once within a document.
            ''' </summary>
            ''' <returns></returns>
            Public Property isIncludeAddon As Boolean = False
            ''' <summary>
            ''' Optional. If set true, the addon being called will be delivered as ah html document, with head, body and html tags. This forces the addon's htmlDocument setting.
            ''' </summary>
            ''' <returns></returns>
            Public Property forceHtmlDocument As Boolean = False
        End Class

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
        <Obsolete("Deprecated, use AppendLog")> Public MustOverride Sub AppendLogFile(ByVal Text As String)
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
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collection file asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFile"></param>
        ''' <returns></returns>
        Public MustOverride Function installCollectionFromFile(privateFile As String) As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Install all addon collections in a folder asynchonously. Optionally delete the folder. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <param name="deleteFolderWhenDone"></param>
        ''' <returns></returns>
        Public MustOverride Function installCollectionsFromFolder(privateFolder As String, deleteFolderWhenDone As Boolean) As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Install all addon collections in a folder asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <returns></returns>
        Public MustOverride Function installCollectionsFromFolder(privateFolder As String) As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="collectionGuid"></param>
        ''' <returns></returns>
        Public MustOverride Function installCollectionFromLibrary(collectionGuid As String) As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collections from an endpoint asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="link"></param>
        ''' <returns></returns>
        Public MustOverride Function installCollectionFromLink(link As String) As Integer
    End Class
End Namespace

