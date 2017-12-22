
Option Strict On
Option Explicit On


Imports Contensive.Core.Controllers
Imports System.Web.Security.FormsAuthentication
Imports System.Guid
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPUtilsClass.ClassId, CPUtilsClass.InterfaceId, CPUtilsClass.EventsId)>
    Public Class CPUtilsClass
        Inherits BaseClasses.CPUtilsBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "BAF47FF8-7D7B-4375-A5BB-06E576AB757B"
        Public Const InterfaceId As String = "78662206-16DF-4C5D-B25E-30292E99EC88"
        Public Const EventsId As String = "88D127A1-BD5C-43C6-8814-BE17CADBF7AC"
#End Region
        '
        Private CP As CPClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByRef CPParentObj As CPClass)
            MyBase.New()
            CP = CPParentObj
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference cp, main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    CP = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Upgrade the current application. If isNewApp is true core tables and content can be created.
        ''' </summary>
        ''' <param name="isNewApp"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Upgrade(isNewApp As Boolean)
            Try
                Throw New ApplicationException("Installation upgrade through the cp interface is deprecated. Please use the command line tool.")
                ' Controllers.appBuilderController.upgrade(CP.core, isNewApp)
            Catch ex As Exception
                CP.core.handleException(ex)
            End Try
        End Sub
        '
        '
        '
        Public Overrides Function ConvertHTML2Text(ByVal Source As String) As String

            If True Then
                ConvertHTML2Text = CP.core.html.convertHTMLToText(Source)
            Else
                ConvertHTML2Text = Source
            End If
        End Function
        '
        '
        '
        Public Overrides Function ConvertText2Html(ByVal Source As String) As String
            If True Then
                ConvertText2Html = CP.core.html.convertTextToHTML(Source)
            Else
                ConvertText2Html = Source
            End If
        End Function

        Public Overrides Function CreateGuid() As String
            Return genericController.createGuid()
        End Function

        Public Overrides Function DecodeUrl(ByVal Url As String) As String
            Return genericController.DecodeURL(Url)
        End Function

        Public Overrides Function EncodeContentForWeb(ByVal Source As String, Optional ByVal ContextContentName As String = "", Optional ByVal ContextRecordID As Integer = 0, Optional ByVal WrapperID As Integer = 0) As String
            Return CP.core.html.convertActiveContentToHtmlForWebRender(Source, ContextContentName, ContextRecordID, 0, "", WrapperID, CPUtilsBaseClass.addonContext.ContextPage)
        End Function

        Public Overrides Function DecodeHtml(ByVal Source As String) As String
            Return genericController.decodeHtml(Source)
        End Function

        Public Overrides Function EncodeHtml(ByVal Source As String) As String
            Dim returnValue As String = ""
            '
            If (Source <> "") Then
                returnValue = genericController.encodeHTML(Source)
            End If
            Return returnValue
        End Function

        Public Overrides Function EncodeUrl(ByVal Source As String) As String
            Return genericController.EncodeURL(Source)
            'If true Then
            '    EncodeUrl = cmc.main_EncodeURL(Source)
            'Else
            '    EncodeUrl = ""
            'End If
        End Function

        Public Overrides Function GetPleaseWaitEnd() As String
            If True Then
                Return CP.core.programFiles.readFile("resources\WaitPageClose.htm")
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetPleaseWaitStart() As String
            If True Then
                Return CP.core.programFiles.readFile("Resources\WaitPageOpen.htm")
            Else
                Return ""
            End If
        End Function


        Public Overrides Sub IISReset()
            If True Then
                Call CP.core.webServer.reset()
            End If
        End Sub
        '
        '
        '
        Public Overrides Function EncodeInteger(ByVal Expression As Object) As Integer
            Return genericController.EncodeInteger(Expression)
        End Function
        '
        '
        '
        Public Overrides Function EncodeNumber(ByVal Expression As Object) As Double
            EncodeNumber = 0
            Try
                If (Expression Is Nothing) Then
                    EncodeNumber = 0
                ElseIf genericController.vbIsNumeric(Expression) Then
                    EncodeNumber = CDbl(Expression)
                ElseIf TypeOf Expression Is Boolean Then
                    If DirectCast(Expression, Boolean) Then
                        EncodeNumber = 1
                    End If
                End If
            Catch ex As Exception
                EncodeNumber = 0
            End Try
        End Function
        '
        '
        '
        Public Overrides Function encodeText(ByVal Expression As Object) As String
            encodeText = ""
            Try
                If (Expression Is Nothing) Then
                    encodeText = ""
                ElseIf (Expression Is DBNull.Value) Then
                    encodeText = ""
                Else
                    encodeText = CStr(Expression)
                End If
            Catch
                encodeText = ""
            End Try

        End Function
        '
        '
        '
        Public Overrides Function EncodeBoolean(ByVal Expression As Object) As Boolean
            EncodeBoolean = False
            Try
                If (Expression Is Nothing) Then
                    EncodeBoolean = False
                ElseIf TypeOf Expression Is Boolean Then
                    EncodeBoolean = DirectCast(Expression, Boolean)
                ElseIf genericController.vbIsNumeric(Expression) Then
                    EncodeBoolean = (CStr(Expression) <> "0")
                ElseIf TypeOf Expression Is String Then
                    Select Case Expression.ToString.ToLower.Trim
                        Case "on", "yes", "true"
                            EncodeBoolean = True
                    End Select
                End If
            Catch ex As Exception
                EncodeBoolean = False
            End Try
        End Function
        '
        '
        '
        Public Overrides Function EncodeDate(ByVal Expression As Object) As Date
            Dim result As Date = Date.MinValue
            Try
                If (Expression Is Nothing) Then
                    result = Date.MinValue
                ElseIf IsDate(Expression) Then
                    result = CDate(Expression)
                End If
                If result < #1/1/1900# Then
                    result = Date.MinValue
                End If
            Catch ex As Exception
                result = Date.MinValue
            End Try
            Return result
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String) As String
            Dim executeConext As New addonExecuteContext() With {
                    .addonType = addonContext.ContextPage
            }
            If (IsNumeric(IdGuidOrName)) Then
                executeConext.errorCaption = "id:" & IdGuidOrName
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, genericController.EncodeInteger(IdGuidOrName)), executeConext)
            ElseIf genericController.isGuid(IdGuidOrName) Then
                executeConext.errorCaption = "guid:" & IdGuidOrName
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, IdGuidOrName), executeConext)
            Else
                executeConext.errorCaption = IdGuidOrName
                Return CP.core.addon.execute(Models.Entity.addonModel.createByName(CP.core, IdGuidOrName), executeConext)
            End If
            'Return CP.core.addon.execute_legacy3(IdGuidOrName, CP.core.docProperties.getLegacyOptionStringFromVar(), 0)
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal WrapperId As Integer) As String
            Dim executeConext As New addonExecuteContext() With {
                    .addonType = addonContext.ContextPage,
                    .wrapperID = WrapperId
            }
            If (IsNumeric(IdGuidOrName)) Then
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, genericController.EncodeInteger(IdGuidOrName)), executeConext)
            ElseIf genericController.isGuid(IdGuidOrName) Then
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, IdGuidOrName), executeConext)
            Else
                Return CP.core.addon.execute(Models.Entity.addonModel.createByName(CP.core, IdGuidOrName), executeConext)
            End If
            'Return CP.core.addon.execute_legacy3(IdGuidOrName, CP.core.docProperties.getLegacyOptionStringFromVar(), WrapperId)
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal context As addonContext) As String
            Dim executeConext As New addonExecuteContext() With {
                    .addonType = context
            }
            If (IsNumeric(IdGuidOrName)) Then
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, genericController.EncodeInteger(IdGuidOrName)), executeConext)
            ElseIf genericController.isGuid(IdGuidOrName) Then
                Return CP.core.addon.execute(Models.Entity.addonModel.create(CP.core, IdGuidOrName), executeConext)
            Else
                Return CP.core.addon.execute(Models.Entity.addonModel.createByName(CP.core, IdGuidOrName), executeConext)
            End If
            'Return CP.core.addon.execute_legacy4(IdGuidOrName, CP.core.docProperties.getLegacyOptionStringFromVar(), context, Nothing)
        End Function

        Public Overrides Function ExecuteAddonAsProcess(ByVal IdGuidOrName As String) As String
            Return CP.core.addon.executeAddonAsProcess(IdGuidOrName, CP.core.docProperties.getLegacyOptionStringFromVar())
        End Function
        <Obsolete("Deprecated, use AppendLog")> Public Overrides Sub AppendLogFile(ByVal Text As String)
            logController.appendLog(CP.core, Text)
        End Sub

        Public Overrides Sub AppendLog(ByVal logFolder As String, ByVal Text As String)
            logController.appendLog(CP.core, Text, logFolder)
        End Sub

        Public Overrides Sub AppendLog(ByVal Text As String)
            logController.appendLog(CP.core, Text)
        End Sub

        Public Overrides Function ConvertLinkToShortLink(ByVal URL As String, ByVal ServerHost As String, ByVal ServerVirtualPath As String) As String
            ConvertLinkToShortLink = genericController.ConvertLinkToShortLink(URL, ServerHost, ServerVirtualPath)
        End Function

        Public Overrides Function ConvertShortLinkToLink(ByVal URL As String, ByVal PathPagePrefix As String) As String
            ConvertShortLinkToLink = genericController.ConvertShortLinkToLink(URL, PathPagePrefix)
        End Function

        Public Overrides Function DecodeGMTDate(ByVal GMTDate As String) As Date
            DecodeGMTDate = genericController.DecodeGMTDate(GMTDate)
        End Function

        Public Overrides Function DecodeResponseVariable(ByVal Source As String) As String
            DecodeResponseVariable = genericController.DecodeResponseVariable(Source)
        End Function

        Public Overrides Function EncodeJavascript(ByVal Source As String) As String
            EncodeJavascript = genericController.EncodeJavascript(Source)
        End Function

        Public Overrides Function EncodeQueryString(ByVal Source As String) As String
            EncodeQueryString = genericController.EncodeQueryString(Source)
        End Function

        Public Overrides Function EncodeRequestVariable(ByVal Source As String) As String
            EncodeRequestVariable = genericController.EncodeRequestVariable(Source)
        End Function

        Public Overrides Function GetArgument(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "", Optional ByVal Delimiter As String = "") As String
            GetArgument = genericController.GetArgument(Name, ArgumentString, DefaultValue, Delimiter)
        End Function

        Public Overrides Function GetFilename(ByVal PathFilename As String) As String
            Dim filename As String = ""
            Dim path As String = ""
            CP.core.privateFiles.splitPathFilename(PathFilename, path, filename)
            Return filename
        End Function

        Public Overrides Function GetFirstNonZeroDate(ByVal Date0 As Date, ByVal Date1 As Date) As Date
            GetFirstNonZeroDate = genericController.GetFirstNonZeroDate(Date0, Date1)
        End Function

        Public Overrides Function GetFirstNonZeroInteger(ByVal Integer0 As Integer, ByVal Integer1 As Integer) As Integer
            GetFirstNonZeroInteger = genericController.GetFirstNonZeroInteger(Integer0, Integer1)
        End Function

        Public Overrides Function GetIntegerString(ByVal Value As Integer, ByVal DigitCount As Integer) As String
            GetIntegerString = genericController.GetIntegerString(Value, DigitCount)
        End Function

        Public Overrides Function GetLine(ByVal Body As String) As String
            GetLine = genericController.getLine(Body)
        End Function

        Public Overrides Function GetListIndex(ByVal Item As String, ByVal ListOfItems As String) As Integer
            GetListIndex = genericController.GetListIndex(Item, ListOfItems)
        End Function

        Public Overrides Function GetProcessID() As Integer
            GetProcessID = genericController.GetProcessID()
        End Function

        Public Overrides Function GetRandomInteger() As Integer
            GetRandomInteger = genericController.GetRandomInteger()
        End Function

        Public Overrides Function IsInDelimitedString(ByVal DelimitedString As String, ByVal TestString As String, ByVal Delimiter As String) As Boolean
            IsInDelimitedString = genericController.IsInDelimitedString(DelimitedString, TestString, Delimiter)
        End Function

        Public Overrides Function ModifyLinkQueryString(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            ModifyLinkQueryString = genericController.ModifyLinkQueryString(Link, QueryName, QueryValue, AddIfMissing)
        End Function

        Public Overrides Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            ModifyQueryString = genericController.ModifyQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
        End Function

        Public Overrides Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            Call genericController.ParseURL(SourceURL, Protocol, Host, Port, Path, Page, QueryString)
        End Sub

        Public Overrides Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            Call genericController.SeparateURL(SourceURL, Protocol, Host, Path, Page, QueryString)
        End Sub

        Public Overrides Function SplitDelimited(ByVal WordList As String, ByVal Delimiter As String) As Object
            SplitDelimited = genericController.SplitDelimited(WordList, Delimiter)
        End Function
        '
        Public Overrides Sub Sleep(timeMSec As Integer)
            System.Threading.Thread.Sleep(timeMSec)
        End Sub
        '
        Public Overrides Function hashMd5(source As String) As String
            Throw New NotImplementedException("hashMd5 not implemented yet")
            'Return HashPasswordForStoringInConfigFile(source, "md5")
        End Function
        '
        Public Overrides Function isGuid(guid As String) As Boolean
            Return genericController.common_isGuid(guid)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collection file asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFile"></param>
        ''' <returns></returns>
        Public Overrides Function installCollectionFromFile(privateFile As String) As Integer
            Dim taskId As Integer = 0
            Dim ignoreUserMessage As String = ""
            Dim ignoreGuid As String = ""
            Call addonInstallClass.InstallCollectionsFromPrivateFile(CP.core, privateFile, ignoreUserMessage, ignoreGuid, False, New List(Of String))
            Return taskId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Install all addon collections in a folder asynchonously. Optionally delete the folder. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <param name="deleteFolderWhenDone"></param>
        ''' <returns></returns>
        Public Overrides Function installCollectionsFromFolder(privateFolder As String, deleteFolderWhenDone As Boolean) As Integer
            Dim taskId As Integer = 0
            Dim ignoreUserMessage As String = ""
            Dim ignoreList As New List(Of String)
            Call addonInstallClass.InstallCollectionsFromPrivateFolder(CP.core, privateFolder, ignoreUserMessage, ignoreList, False, New List(Of String))
            Return taskId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Install all addon collections in a folder asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <returns></returns>
        Public Overrides Function installCollectionsFromFolder(privateFolder As String) As Integer
            Return installCollectionsFromFolder(privateFolder, False)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <param name="deleteFolderWhenDone"></param>
        ''' <returns></returns>
        Public Overrides Function installCollectionFromLibrary(collectionGuid As String) As Integer
            Dim taskId As Integer = 0
            Dim ignoreUserMessage As String = ""
            Call addonInstallClass.installCollectionFromRemoteRepo(CP.core, collectionGuid, ignoreUserMessage, "", False, New List(Of String))
            Return taskId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Install an addon collections from an endpoint asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <param name="deleteFolderWhenDone"></param>
        ''' <returns></returns>
        Public Overrides Function installCollectionFromLink(link As String) As Integer
            Throw New NotImplementedException()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Converts html content to the wysiwyg editor compatible format that includes edit icons for addons. Use this to convert the html content added to wysiwyg editors. Use EncodeHtmlFromWysiwygEditor() before saving back to Db.
        ''' </summary>
        ''' <param name="Source"></param>
        ''' <returns></returns>
        Public Overrides Function EncodeHtmlForWysiwygEditor(Source As String) As String
            Return CP.core.html.convertActiveContentToHtmlForWysiwygEditor(Source)
            'Return CP.core.html.convertActiveContent_internal(Source, 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, False, Nothing, False)
            'Return CP.core.encodeContent9(Source, 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", 1)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Converts html content from wysiwyg editors to be saved. See EncodeHtmlForWysiwygEditor() for more details.
        ''' </summary>
        ''' <param name="Source"></param>
        ''' <returns></returns>
        Public Overrides Function DecodeHtmlFromWysiwygEditor(Source As String) As String
            Return CP.core.html.convertEditorResponseToActiveContent(Source)
            'Throw New NotImplementedException()
        End Function
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.utils, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
#Region " IDisposable Support "
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