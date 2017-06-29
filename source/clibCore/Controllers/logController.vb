
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
Imports Contensive.Core.Controllers
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class logController
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- private instance storage
        '
        'Private remoteCacheDisabled As Boolean
        '
        Public Shared Function LogFileCopyPrep(ByVal Source As String) As String
            Dim Copy As String
            Copy = Source
            Copy = genericController.vbReplace(Copy, vbCrLf, " ")
            Copy = genericController.vbReplace(Copy, vbLf, " ")
            Copy = genericController.vbReplace(Copy, vbCr, " ")
            LogFileCopyPrep = Copy
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' add the log line to a log file with the folder and prefix
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="LogLine"></param>
        ''' <param name="LogFolder"></param>
        ''' <param name="LogNamePrefix"></param>
        ''' <param name="allowErrorHandling"></param>
        ''' <remarks></remarks>
        Public Shared Sub appendLog(cpCore As coreClass, ByVal LogLine As String, Optional ByVal LogFolder As String = "", Optional ByVal LogNamePrefix As String = "", Optional allowErrorHandling As Boolean = True)
            Try
                If (String.IsNullOrEmpty(LogFolder) Or cpCore.serverConfig.enableLogging) Then
                    '
                    ' -- log in root folder or if enable logging it enabled
                    Dim logPath As String
                    Dim MonthNumber As Integer
                    Dim DayNumber As Integer
                    Dim FilenameNoExt As String
                    Dim PathFilenameNoExt As String
                    Dim FileSize As Integer
                    Dim RetryCnt As Integer
                    Dim SaveOK As Boolean
                    Dim FileSuffix As String
                    Dim threadId As Integer = System.Threading.Thread.CurrentThread.ManagedThreadId
                    Dim threadName As String = Format(threadId, "00000000")
                    Dim fileSystem As fileController = Nothing
                    '
                    Try
                        If (cpCore.serverConfig IsNot Nothing) Then
                            If (cpCore.serverConfig.appConfig IsNot Nothing) Then
                                '
                                ' -- use app log space
                                fileSystem = cpCore.privateFiles
                            End If
                        End If
                        If (fileSystem Is Nothing) Then
                            '
                            ' -- no app or no server, use program data files
                            fileSystem = cpCore.programDataFiles
                        End If
                        DayNumber = Day(Now)
                        MonthNumber = Month(Now)
                        FilenameNoExt = getDateString(Now)
                        logPath = LogFolder
                        If logPath <> "" Then
                            logPath = logPath & "\"
                        End If
                        logPath = "logs\" & logPath
                        '
                        ' check for serverconfig, then for appConfig, else use programdata folder
                        '
                        ' logPathRoot = privatefiles.rootLocalPath
                        If Not fileSystem.pathExists(logPath) Then
                            Call fileSystem.createPath(logPath)
                        Else
                            Dim logFiles As IO.FileInfo() = fileSystem.getFileList(logPath)
                            For Each fileInfo As IO.FileInfo In logFiles
                                If fileInfo.Name.ToLower = FilenameNoExt.ToLower & ".log" Then
                                    FileSize = CInt(fileInfo.Length)
                                    Exit For
                                End If
                            Next
                        End If
                        PathFilenameNoExt = logPath & FilenameNoExt
                        '
                        ' -- add to log file
                        If FileSize < 10000000 Then
                            RetryCnt = 0
                            SaveOK = False
                            FileSuffix = ""
                            Do While (Not SaveOK) And (RetryCnt < 10)
                                SaveOK = True
                                Try
                                    Dim absContent As String = LogFileCopyPrep(FormatDateTime(Now(), vbGeneralDate)) & vbTab & threadName & vbTab & LogLine & vbCrLf
                                    fileSystem.appendFile(PathFilenameNoExt & FileSuffix & ".log", absContent)
                                Catch ex As IO.IOException
                                    '
                                    ' permission denied - happens when more then one process are writing at once, go to the next suffix
                                    '
                                    FileSuffix = "-" & CStr(RetryCnt + 1)
                                    RetryCnt = RetryCnt + 1
                                    SaveOK = False
                                Catch ex As Exception
                                    '
                                    ' unknown error
                                    '
                                    FileSuffix = "-" & CStr(RetryCnt + 1)
                                    RetryCnt = RetryCnt + 1
                                    SaveOK = False
                                End Try
                            Loop
                        End If
                    Catch ex As Exception
                        ' -- ignore errors in error handling
                    End Try
                End If
            Catch ex As Exception
                ' -- ignore errors in error handling
            Finally
                '
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Append log, use the legacy row with tab delimited context
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ContensiveAppName"></param>
        ''' <param name="contextDescription"></param>
        ''' <param name="processName"></param>
        ''' <param name="ClassName"></param>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <param name="ErrorTrap"></param>
        ''' <param name="ResumeNextAfterLogging"></param>
        ''' <param name="URL"></param>
        ''' <param name="LogFolder"></param>
        ''' <param name="LogNamePrefix"></param>
        ''' <remarks></remarks>
        Public Shared Sub appendLogWithLegacyRow(cpCore As coreClass, ByVal ContensiveAppName As String, ByVal contextDescription As String, ByVal processName As String, ByVal ClassName As String, ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal ErrorTrap As Boolean, ByVal ResumeNextAfterLogging As Boolean, ByVal URL As String, ByVal LogFolder As String, ByVal LogNamePrefix As String)
            Try
                Dim ErrorMessage As String
                Dim LogLine As String
                Dim ResumeMessage As String
                '
                If ErrorTrap Then
                    ErrorMessage = "Error Trap"
                Else
                    ErrorMessage = "Log Entry"
                End If
                '
                If ResumeNextAfterLogging Then
                    ResumeMessage = "Resume after logging"
                Else
                    ResumeMessage = "Abort after logging"
                End If
                '
                LogLine = "" _
                    & LogFileCopyPrep(ContensiveAppName) _
                    & vbTab & LogFileCopyPrep(processName) _
                    & vbTab & LogFileCopyPrep(ClassName) _
                    & vbTab & LogFileCopyPrep(MethodName) _
                    & vbTab & LogFileCopyPrep(contextDescription) _
                    & vbTab & LogFileCopyPrep(ErrorMessage) _
                    & vbTab & LogFileCopyPrep(ResumeMessage) _
                    & vbTab & LogFileCopyPrep(ErrSource) _
                    & vbTab & LogFileCopyPrep(ErrNumber.ToString) _
                    & vbTab & LogFileCopyPrep(ErrDescription) _
                    & vbTab & LogFileCopyPrep(URL) _
                    & ""
                '
                appendLog(cpCore, LogLine, LogFolder, LogNamePrefix)
            Catch ex As Exception

            End Try
        End Sub        '

        '
        '====================================================================================================
        ''' <summary>
        ''' Create a string with year, month, date in the form 20151206
        ''' </summary>
        ''' <param name="sourceDate"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getDateString(sourceDate As Date) As String
            Return sourceDate.Year & sourceDate.Month.ToString.PadLeft(2, CChar("0")) & sourceDate.Day.ToString.PadLeft(2, CChar("0"))
        End Function
        '
        '=====================================================================================================
        '   Insert into the ActivityLog
        '=====================================================================================================
        '
        Public Shared Sub logActivity(cpcore As coreClass, ByVal Message As String, ByVal ByMemberID As Integer, ByVal SubjectMemberID As Integer, ByVal SubjectOrganizationID As Integer, Optional ByVal Link As String = "", Optional ByVal VisitorID As Integer = 0, Optional ByVal VisitID As Integer = 0)
            On Error GoTo ErrorTrap 'Const Tn = "LogActivity2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            '
            CS = cpcore.db.cs_insertRecord("Activity Log", ByMemberID)
            If cpcore.db.cs_ok(CS) Then
                Call cpcore.db.cs_set(CS, "MemberID", SubjectMemberID)
                Call cpcore.db.cs_set(CS, "OrganizationID", SubjectOrganizationID)
                Call cpcore.db.cs_set(CS, "Message", Message)
                Call cpcore.db.cs_set(CS, "Link", Link)
                Call cpcore.db.cs_set(CS, "VisitorID", VisitorID)
                Call cpcore.db.cs_set(CS, "VisitID", VisitID)
            End If
            Call cpcore.db.cs_Close(CS)
            '
            Exit Sub
            '
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Sub
        '
        '
        Public Shared Sub logActivity2(cpcore As coreClass, Message As String, SubjectMemberID As Integer, SubjectOrganizationID As Integer)
            Call logActivity(cpcore, Message, cpcore.authContext.user.ID, SubjectMemberID, SubjectOrganizationID, cpcore.webServer.webServerIO_ServerLink, cpcore.authContext.visitor.ID, cpcore.authContext.visit.ID)
        End Sub
        '
        '
        '
        Friend Shared Sub log_appendLogPageNotFound(cpCore As coreClass, PageNotFoundLink As String)
            Try
                Call appendLog(cpCore, """" & FormatDateTime(cpCore.app_startTime, vbGeneralDate) & """,""App=" & cpCore.serverConfig.appConfig.name & """,""main_VisitId=" & cpCore.authContext.visit.ID & """,""" & PageNotFoundLink & """,""Referrer=" & cpCore.webServer.requestReferrer & """", "performance", "pagenotfound")
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
    End Class
    '
End Namespace