
Option Strict On
Option Explicit On

Imports cl = Contensive.Core.coreCommonModule
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
        Public Const ClassId As String = ""
        Public Const InterfaceId As String = ""
        Public Const EventsId As String = ""
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
                Dim builder As New coreBuilderClass(CP.core)
                builder.upgrade(isNewApp)
            Catch ex As Exception
                CP.core.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '
        '
        Public Overrides Function ConvertHTML2Text(ByVal Source As String) As String

            If True Then
                ConvertHTML2Text = CP.core.main_ConvertHTML2Text(Source)
            Else
                ConvertHTML2Text = Source
            End If
        End Function
        '
        '
        '
        Public Overrides Function ConvertText2Html(ByVal Source As String) As String
            If True Then
                ConvertText2Html = CP.core.html_convertText2HTML(Source)
            Else
                ConvertText2Html = Source
            End If
        End Function

        Public Overrides Function CreateGuid() As String
            'Dim guid As Guid = guid.NewGuid()
            Return "{" & Guid.NewGuid().ToString & "}"
            'Dim guid As CSGUID.GUIDGenerator = New CSGUID.GUIDGenerator
            'Return guid.CreateGUID("")
            'If true Then
            '    CreateGuid = cmc.main_CreateGuid()
            'Else
            '    CreateGuid = ""
            'End If
        End Function

        Public Overrides Function DecodeUrl(ByVal Url As String) As String
            Return cl.DecodeURL(Url)
            'return System.Web.HttpServerUtility.
            'If true Then
            '    DecodeUrl = cmc.main_DecodeUrl(Url)
            'Else
            '    DecodeUrl = ""
            'End If
        End Function

        Public Overrides Function EncodeContentForWeb(ByVal Source As String, Optional ByVal ContextContentName As String = "", Optional ByVal ContextRecordID As Integer = 0, Optional ByVal WrapperID As Integer = 0) As String
            If True Then
                EncodeContentForWeb = CP.core.html_encodeContentForWeb(Source, ContextContentName, ContextRecordID, "", WrapperID)
            Else
                EncodeContentForWeb = ""
            End If
        End Function

        Public Overrides Function DecodeHtml(ByVal Source As String) As String
            Return cl.decodeHtml(Source)
            'If true Then
            '    DecodeHtml = cmc.main_DecodeHtml(Source)
            'Else
            '    DecodeHtml = ""
            'End If
        End Function

        Public Overrides Function EncodeHtml(ByVal Source As String) As String
            Dim returnValue As String = ""
            '
            If (Source <> "") Then
                returnValue = CP.core.html.html_EncodeHTML(Source)
            End If
            Return returnValue
        End Function

        Public Overrides Function EncodeUrl(ByVal Source As String) As String
            Return cl.EncodeURL(Source)
            'If true Then
            '    EncodeUrl = cmc.main_EncodeURL(Source)
            'Else
            '    EncodeUrl = ""
            'End If
        End Function

        Public Overrides Function GetPleaseWaitEnd() As String
            If True Then
                Return CP.core.main_GetPleaseWaitEnd
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetPleaseWaitStart() As String
            If True Then
                Return CP.core.main_GetPleaseWaitStart
            Else
                Return ""
            End If
        End Function


        Public Overrides Sub IISReset()
            If True Then
                Call CP.core.main_IISReset()
            End If
        End Sub
        '
        '
        '
        Public Overrides Function EncodeInteger(ByVal Expression As Object) As Integer
            EncodeInteger = 0
            Try
                If (Expression Is Nothing) Then
                    EncodeInteger = 0
                ElseIf vbIsNumeric(Expression) Then
                    EncodeInteger = CInt(Expression)
                ElseIf TypeOf Expression Is Boolean Then
                    If DirectCast(Expression, Boolean) Then
                        EncodeInteger = 1
                    End If
                End If
            Catch ex As Exception
                EncodeInteger = 0
            End Try
        End Function
        '
        '
        '
        Public Overrides Function EncodeNumber(ByVal Expression As Object) As Double
            EncodeNumber = 0
            Try
                If (Expression Is Nothing) Then
                    EncodeNumber = 0
                ElseIf vbIsNumeric(Expression) Then
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
        Public Overrides Function EncodeText(ByVal Expression As Object) As String
            EncodeText = ""
            Try
                If (Expression Is Nothing) Then
                    EncodeText = ""
                ElseIf (Expression Is DBNull.Value) Then
                    EncodeText = ""
                Else
                    EncodeText = CStr(Expression)
                End If
            Catch
                EncodeText = ""
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
                ElseIf vbIsNumeric(Expression) Then
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
            EncodeDate = Date.MinValue
            Try
                If (Expression Is Nothing) Then
                    EncodeDate = Date.MinValue
                ElseIf IsDate(Expression) Then
                    EncodeDate = CDate(Expression)
                End If
                If EncodeDate < #1/1/1900# Then
                    EncodeDate = Date.MinValue
                End If
            Catch ex As Exception
                EncodeDate = Date.MinValue
            End Try
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String) As String
            Return CP.core.addon.execute_legacy3(IdGuidOrName, CP.core.getLegacyOptionStringFromVar(), 0, Nothing)
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal WrapperId As Integer) As String
            Return CP.core.addon.execute_legacy3(IdGuidOrName, CP.core.getLegacyOptionStringFromVar(), WrapperId, Nothing)
        End Function
        '
        '
        '
        Public Overrides Function ExecuteAddon(ByVal IdGuidOrName As String, ByVal context As addonContext) As String
            Return CP.core.addon.execute_legacy4(IdGuidOrName, CP.core.getLegacyOptionStringFromVar(), context, Nothing)
        End Function

        Public Overrides Function ExecuteAddonAsProcess(ByVal IdGuidOrName As String) As String
            Return CP.core.addon.executeAddonAsProcess(IdGuidOrName, CP.core.getLegacyOptionStringFromVar())
        End Function


        Public Overrides Sub AppendLog(ByVal logFolder As String, ByVal Text As String)
            CP.core.log_appendLog(Text, logFolder)
        End Sub

        Public Overrides Sub AppendLog(ByVal Text As String)
            CP.core.log_appendLog(Text)
        End Sub

        Public Overrides Function ConvertLinkToShortLink(ByVal URL As String, ByVal ServerHost As String, ByVal ServerVirtualPath As String) As String
            ConvertLinkToShortLink = cl.ConvertLinkToShortLink(URL, ServerHost, ServerVirtualPath)
        End Function

        Public Overrides Function ConvertShortLinkToLink(ByVal URL As String, ByVal PathPagePrefix As String) As String
            ConvertShortLinkToLink = cl.ConvertShortLinkToLink(URL, PathPagePrefix)
        End Function

        Public Overrides Function DecodeGMTDate(ByVal GMTDate As String) As Date
            DecodeGMTDate = cl.DecodeGMTDate(GMTDate)
        End Function

        Public Overrides Function DecodeResponseVariable(ByVal Source As String) As String
            DecodeResponseVariable = cl.DecodeResponseVariable(Source)
        End Function

        Public Overrides Function EncodeJavascript(ByVal Source As String) As String
            EncodeJavascript = cl.EncodeJavascript(Source)
        End Function

        Public Overrides Function EncodeQueryString(ByVal Source As String) As String
            EncodeQueryString = cl.EncodeQueryString(Source)
        End Function

        Public Overrides Function EncodeRequestVariable(ByVal Source As String) As String
            EncodeRequestVariable = cl.EncodeRequestVariable(Source)
        End Function

        Public Overrides Function GetArgument(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "", Optional ByVal Delimiter As String = "") As String
            GetArgument = cl.GetArgument(Name, ArgumentString, DefaultValue, Delimiter)
        End Function

        Public Overrides Function GetFilename(ByVal PathFilename As String) As String
            GetFilename = cl.GetFilename(PathFilename)
        End Function

        Public Overrides Function GetFirstNonZeroDate(ByVal Date0 As Date, ByVal Date1 As Date) As Date
            GetFirstNonZeroDate = cl.GetFirstNonZeroDate(Date0, Date1)
        End Function

        Public Overrides Function GetFirstNonZeroInteger(ByVal Integer0 As Integer, ByVal Integer1 As Integer) As Integer
            GetFirstNonZeroInteger = cl.GetFirstNonZeroInteger(Integer0, Integer1)
        End Function

        Public Overrides Function GetIntegerString(ByVal Value As Integer, ByVal DigitCount As Integer) As String
            GetIntegerString = cl.GetIntegerString(Value, DigitCount)
        End Function

        Public Overrides Function GetLine(ByVal Body As String) As String
            GetLine = cl.getLine(Body)
        End Function

        Public Overrides Function GetListIndex(ByVal Item As String, ByVal ListOfItems As String) As Integer
            GetListIndex = cl.GetListIndex(Item, ListOfItems)
        End Function

        Public Overrides Function GetProcessID() As Integer
            GetProcessID = cl.GetProcessID()
        End Function

        Public Overrides Function GetRandomInteger() As Integer
            GetRandomInteger = cl.GetRandomInteger()
        End Function

        Public Overrides Function IsInDelimitedString(ByVal DelimitedString As String, ByVal TestString As String, ByVal Delimiter As String) As Boolean
            IsInDelimitedString = cl.IsInDelimitedString(DelimitedString, TestString, Delimiter)
        End Function

        Public Overrides Function ModifyLinkQueryString(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            ModifyLinkQueryString = cl.ModifyLinkQueryString(Link, QueryName, QueryValue, AddIfMissing)
        End Function

        Public Overrides Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            ModifyQueryString = cl.ModifyQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
        End Function

        Public Overrides Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            Call cl.ParseURL(SourceURL, Protocol, Host, Port, Path, Page, QueryString)
        End Sub

        Public Overrides Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            Call cl.SeparateURL(SourceURL, Protocol, Host, Path, Page, QueryString)
        End Sub

        Public Overrides Function SplitDelimited(ByVal WordList As String, ByVal Delimiter As String) As Object
            SplitDelimited = cl.SplitDelimited(WordList, Delimiter)
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
            Return CP.core.common_isGuid(guid)
        End Function
        '
        Public Overrides Function installCollectionAsyncFromFile(privateFile As String) As Integer
            Throw New NotImplementedException()
        End Function
        '
        Public Overrides Function installCollectionAsyncFromLibrary(collectionGuid As String) As Integer
            Throw New NotImplementedException()
        End Function
        '
        Public Overrides Function installCollectionAsyncFromLink(link As String) As Integer
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
            Return CP.core.html_encodeContent10(Source, 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, False, Nothing, False)
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
            Return CP.core.html_DecodeContent(Source)
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