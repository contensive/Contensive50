
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
    Public Class errorController
        Implements IDisposable
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        '==========================================================================
        '   Add on to the common error message
        '==========================================================================
        '
        Public Shared Sub error_AddUserError(cpCore As coreClass, ByVal Message As String)
            If (InStr(1, cpCore.debug_iUserError, Message, vbTextCompare) = 0) Then
                cpCore.debug_iUserError = cpCore.debug_iUserError & cr & "<li class=""ccError"">" & genericController.encodeText(Message) & "</LI>"
            End If
        End Sub
        '
        '==========================================================================
        '   main_Get The user error messages
        '       If there are none, return ""
        '==========================================================================
        '
        Public Shared Function error_GetUserError(cpcore As coreClass) As String
            error_GetUserError = genericController.encodeText(cpcore.debug_iUserError)
            If error_GetUserError <> "" Then
                error_GetUserError = "<ul class=""ccError"">" & genericController.htmlIndent(error_GetUserError) & cr & "</ul>"
                error_GetUserError = UserErrorHeadline & "" & error_GetUserError
                cpcore.debug_iUserError = ""
            End If
        End Function

        '
        '==========================================================================================
        ''' <summary>
        ''' return an html ul list of each eception produced during this document.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getDocExceptionHtmlList(cpcore As coreClass) As String
            Dim returnHtmlList As String = ""
            Try
                If Not cpcore.errList Is Nothing Then
                    If cpcore.errList.Count > 0 Then
                        For Each exMsg As String In cpcore.errList
                            returnHtmlList &= cr2 & "<li class=""ccExceptionListRow"">" & cr3 & cpcore.htmlDoc.html_convertText2HTML(exMsg) & cr2 & "</li>"
                        Next
                        returnHtmlList = cr & "<ul class=""ccExceptionList"">" & returnHtmlList & cr & "</ul>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnHtmlList
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