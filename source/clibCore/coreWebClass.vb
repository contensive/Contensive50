
Option Explicit On
Option Strict On

Imports Microsoft.Web.Administration

Namespace Contensive.Core
    ''' <summary>
    ''' Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    ''' </summary>
    Public Class coreWebClass
        '
        Dim cpCore As cpCoreClass
        '
        '   State values that must be initialized before Init()
        '   Everything else is derived from these
        '
        Public RequestLanguage As String = ""            ' set externally from HTTP_Accept_LANGUAGE
        Public requestHttpAccept As String = ""
        Public requestHttpAcceptCharset As String = ""
        Public requestHttpProfile As String = ""
        Public requestxWapProfile As String = ""
        Public requestHTTPVia As String = ""                   ' informs the server of proxies used during the request
        Public requestHTTPFrom As String = ""                  ' contains the email address of the requestor
        Public requestPathPage As String = ""             ' The Path and Page part of the current URI
        Public requestReferrer As String = ""
        Public requestDomain As String = ""                 ' The Host part of the current URI
        Public requestSecure As Boolean = False          ' Set in InitASPEnvironment, true if https
        Public requestRemoteIP As String = ""              '
        Public requestBrowser As String = ""               ' The browser for this visit
        Public requestQueryString As String = ""          ' The QueryString of the current URI
        Public requestFormUseBinaryHeader As Boolean = False              ' When set true with RequestNameBinaryRead=true, InitEnvironment reads the form in with a binary read
        Public requestFormBinaryHeader As Byte() ' Object        ' For asp pages, this is the full multipart header
        Public requestForm As String = ""                 ' String from an HTML form post - buffered to remove passwords
        Public requestFormFiles As String = ""            ' String from an HTML form post
        Public requestCookies As String = ""              ' Set in InitASPEnvironment, the full cookie string
        Public requestSpaceAsUnderscore As Boolean = False ' when true, is it assumed that dots in request variable names will convert
        Public requestDotAsUnderscore As Boolean = False '   (php converts spaces and dots to underscores)
        Public requestLinkSource As String = ""
        '
        '====================================================================================================
        '
        Public Sub New(cpCore As cpCoreClass)
            MyBase.New
            Me.cpCore = cpCore
        End Sub
        '
        '=======================================================================================
        '   IIS Reset
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '
        Public Sub reset()
            Try
                Dim Cmd As String
                Dim arg As String
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                LogFilename = "Temp\" & EncodeText(GetRandomInteger()) & ".Log"
                Cmd = "IISReset.exe"
                arg = "/restart >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, arg, True)
                Copy = cpCore.app.privateFiles.ReadFile(LogFilename)
                Call cpCore.app.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=======================================================================================
        '   Stop IIS
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '
        Public Sub [stop]()
            Try
                Dim Cmd As String
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                LogFilename = "Temp\" & EncodeText(GetRandomInteger()) & ".Log"
                Cmd = "%comspec% /c IISReset /stop >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.app.privateFiles.ReadFile(LogFilename)
                Call cpCore.app.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=======================================================================================
        '   Start IIS
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '=======================================================================================
        '
        Public Sub start()
            Try
                Dim Cmd As String
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                Cmd = "%comspec% /c IISReset /start >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.app.privateFiles.ReadFile(LogFilename)
                Call cpCore.app.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=======================================================================================
        ' recycle iis process
        '
        Public Sub recycle(appName As String)
            Try
                Dim serverManager As ServerManager
                Dim appPoolColl As ApplicationPoolCollection
                '
                serverManager = New ServerManager
                appPoolColl = serverManager.ApplicationPools
                For Each appPool As ApplicationPool In appPoolColl
                    If appPool.Name.ToLower = appName.ToLower Then
                        If appPool.Start = ObjectState.Started Then
                            appPool.Recycle()
                        End If
                    End If
                Next
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
    End Class
End Namespace