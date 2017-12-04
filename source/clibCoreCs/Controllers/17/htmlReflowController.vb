
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class htmlReflowController
        Implements IDisposable
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- objects constructed that must be disposed
        '
        'Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- private instance storage
        '
        'Private remoteCacheDisabled As Boolean
        Public Shared Function reflow(cpCore As coreClass, source As String) As String
            Return source
            'Dim returnHtml As String = ""

            'If cpCore.siteProperties.getBoolean("AutoHTMLFormatting") Then
            '    Dim indentCnt As Integer = 0
            '    Dim Parse As htmlParserController = New htmlParserController(cpCore)
            '    Call Parse.Load(source)
            '    If Parse.ElementCount > 0 Then
            '        For Ptr = 0 To Parse.ElementCount - 1
            '            If Not Parse.IsTag(Ptr) Then
            '                Dim Content As String = Parse.Text(Ptr)
            '                If BlockFormatting Then
            '                    Result.Add(Content)
            '                Else
            '                    If Content <> "" Then
            '                        If Trim(Content) <> "" Then
            '                            Result.Add(ContentIndent)
            '                            ContentIndent = ""
            '                        End If
            '                        Content = genericController.vbReplace(Content, vbCrLf, " ")
            '                        Content = genericController.vbReplace(Content, vbTab, " ")
            '                        Content = genericController.vbReplace(Content, vbCr, " ")
            '                        Content = genericController.vbReplace(Content, vbLf, " ")
            '                        Result.Add(Content)
            '                        ContentCnt = ContentCnt + 1
            '                    End If
            '                End If
            '            Else
            '                Select Case genericController.vbLCase(Parse.TagName(Ptr))
            '                    Case "pre", "script"
            '                        '
            '                        ' End block formating
            '                        '
            '                        Result.Add(vbCrLf & Parse.Text(Ptr))
            '                        BlockFormatting = True
            '                    Case "/pre", "/script"
            '                        '
            '                        ' end block formating
            '                        '
            '                        Result.Add(Parse.Text(Ptr) & vbCrLf)
            '                        BlockFormatting = False
            '                    Case Else
            '                        If BlockFormatting Then
            '                            '
            '                            ' formatting is blocked
            '                            '
            '                            Result.Add(Parse.Text(Ptr))
            '                        Else
            '                            '
            '                            ' format the tag
            '                            '
            '                            Select Case genericController.vbLCase(Parse.TagName(Ptr))
            '                                Case "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "br"
            '                                    '
            '                                    ' new line
            '                                    '
            '                                    Result.Add(vbCrLf & New String(CChar(vbTab), indentCnt) & Parse.Text(Ptr))
            '                                Case "div", "td", "table", "tr", "tbody", "ol", "ul", "form"
            '                                    '
            '                                    ' new line and +indent
            '                                    '
            '                                    Result.Add(vbCrLf & New String(CChar(vbTab), indentCnt) & Parse.Text(Ptr))
            '                                    indentCnt = indentCnt + 1
            '                                    ContentIndent = vbCrLf & New String(CChar(vbTab), indentCnt)
            '                                    ContentCnt = 0
            '                                Case "/div", "/td", "/table", "/tr", "/tbody", "/ol", "/ul", "/form"
            '                                    '
            '                                    ' new line and -indent
            '                                    '
            '                                    indentCnt = indentCnt - 1
            '                                    If indentCnt < 0 Then
            '                                        indentCnt = 0
            '                                        '
            '                                        ' Add to 'Asset Errors' Table - a merge with Spider Doc Errors
            '                                        '
            '                                    End If
            '                                    If ContentCnt = 0 Then
            '                                        Result.Add(Parse.Text(Ptr))
            '                                    Else
            '                                        Result.Add(vbCrLf & New String(CChar(vbTab), indentCnt) & Parse.Text(Ptr))
            '                                    End If
            '                                    ContentCnt = ContentCnt + 1
            '                                Case Else
            '                                    '
            '                                    ' tag that acts like content
            '                                    '
            '                                    Content = Parse.Text(Ptr)
            '                                    If Content <> "" Then
            '                                        Result.Add(ContentIndent & Content)
            '                                        ContentIndent = ""
            '                                    End If
            '                                    ContentCnt = ContentCnt + 1
            '                            End Select
            '                        End If
            '                End Select
            '            End If
            '        Next
            '        If indentCnt <> 0 Then
            '            '
            '            ' Add to 'Asset Errors' Table - a merge with Spider Doc Errors
            '            '
            '            'Call main_AppendClassErrorLog("cpCoreClass(" & appEnvironment.name & ").GetHTMLBody AutoIndent error. At the end of the document, the last tag was still indented (more start tags than end tags). Link=[" & genericController.decodeHtml(main_ServerLink) & "], ")
            '        End If
            '        returnBody = Result.Text
            '    End If
            ' End If

        End Function
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace