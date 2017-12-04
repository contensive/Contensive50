
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
                '
                Dim threadId As Integer = System.Threading.Thread.CurrentThread.ManagedThreadId
                Dim threadName As String = Format(threadId, "00000000")
                Dim absContent As String = LogFileCopyPrep(FormatDateTime(Now(), vbGeneralDate)) & vbTab & "thread:" & threadName & vbTab & LogLine & vbCrLf
                Console.WriteLine(LogLine)
                '
                If (String.IsNullOrEmpty(LogFolder) Or cpCore.serverConfig.enableLogging) Then
                    Try
                        Dim fileSystem As fileController = Nothing
                        If (cpCore.serverConfig IsNot Nothing) Then
                            If (cpCore.serverConfig.appConfig IsNot Nothing) Then
                                '
                                ' -- use app log space
                                FileSystem = cpCore.privateFiles
                            End If
                        End If
                        If (FileSystem Is Nothing) Then
                            '
                            ' -- no app or no server, use program data files
                            FileSystem = cpCore.programDataFiles
                        End If
                        Dim FilenameNoExt As String = getDateString(Now)
                        Dim logPath As String = LogFolder
                        If logPath <> "" Then
                            logPath = logPath & "\"
                        End If
                        logPath = "logs\" & logPath
                        '
                        ' check for serverconfig, then for appConfig, else use programdata folder
                        '
                        Dim FileSize As Integer = 0
                        If Not FileSystem.pathExists(logPath) Then
                            Call FileSystem.createPath(logPath)
                        Else
                            Dim logFiles As IO.FileInfo() = FileSystem.getFileList(logPath)
                            For Each fileInfo As IO.FileInfo In logFiles
                                If fileInfo.Name.ToLower = FilenameNoExt.ToLower & ".log" Then
                                    FileSize = CInt(fileInfo.Length)
                                    Exit For
                                End If
                            Next
                        End If
                        Dim PathFilenameNoExt As String = logPath & FilenameNoExt
                        '
                        ' -- add to log file
                        If FileSize < 10000000 Then
                            Dim RetryCnt As Integer = 0
                            Dim SaveOK As Boolean = False
                            Dim FileSuffix As String = ""
                            Do While (Not SaveOK) And (RetryCnt < 10)
                                SaveOK = True
                                Try
                                    FileSystem.appendFile(PathFilenameNoExt & FileSuffix & ".log", absContent)
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
            CS = cpcore.db.csInsertRecord("Activity Log", ByMemberID)
            If cpcore.db.csOk(CS) Then
                Call cpcore.db.csSet(CS, "MemberID", SubjectMemberID)
                Call cpcore.db.csSet(CS, "OrganizationID", SubjectOrganizationID)
                Call cpcore.db.csSet(CS, "Message", Message)
                Call cpcore.db.csSet(CS, "Link", Link)
                Call cpcore.db.csSet(CS, "VisitorID", VisitorID)
                Call cpcore.db.csSet(CS, "VisitID", VisitID)
            End If
            Call cpcore.db.csClose(CS)
            '
            Exit Sub
            '
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Sub
        '
        '
        Public Shared Sub logActivity2(cpcore As coreClass, Message As String, SubjectMemberID As Integer, SubjectOrganizationID As Integer)
            Call logActivity(cpcore, Message, cpCore.doc.authContext.user.id, SubjectMemberID, SubjectOrganizationID, cpcore.webServer.requestUrl, cpCore.doc.authContext.visitor.ID, cpCore.doc.authContext.visit.id)
        End Sub
        '
        '
        '
        Friend Shared Sub log_appendLogPageNotFound(cpCore As coreClass, PageNotFoundLink As String)
            Try
                Call appendLog(cpCore, """" & FormatDateTime(cpCore.doc.profileStartTime, vbGeneralDate) & """,""App=" & cpCore.serverConfig.appConfig.name & """,""main_VisitId=" & cpCore.doc.authContext.visit.id & """,""" & PageNotFoundLink & """,""Referrer=" & cpCore.webServer.requestReferrer & """", "performance", "pagenotfound")
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '================================================================================================
        '   Report Warning
        '       A warning is logged in the site warnings log
        '           name - a generic description of the warning
        '               "bad link found on page"
        '           short description - a <255 character cause
        '               "bad link http://thisisabadlink.com"
        '           location - the URL, service or process that caused the problem
        '               "http://goodpageThankHasBadLink.com"
        '           pageid - the record id of the bad page.
        '               "http://goodpageThankHasBadLink.com"
        '           description - a specific description
        '               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           generalKey - a generic string that describes the warning. the warning report
        '               will display one line for each generalKey (name matches guid)
        '               like "bad link"
        '           specificKey - a string created by the addon logging so it does not continue to log exactly the
        '               same warning over and over. If there are 100 different link not found warnings,
        '               there should be 100 entires with the same guid and name, but 100 different keys. If the
        '               an identical key is found the count increments.
        '               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           count - the number of times the key was attempted to add. "This error was reported 100 times"
        '================================================================================================
        '
        Public Shared Sub csv_reportWarning(cpcore As coreClass, Name As String, shortDescription As String, location As String, PageID As Integer, Description As String, generalKey As String, specificKey As String)
            Dim SQL As String
            Dim warningId As Integer
            Dim CS As Integer
            '
            warningId = 0
            SQL = "select top 1 ID from ccSiteWarnings" _
                & " where (generalKey=" & cpcore.db.encodeSQLText(generalKey) & ")" _
                & " and(specificKey=" & cpcore.db.encodeSQLText(specificKey) & ")" _
                & ""
            Dim dt As DataTable
            dt = cpcore.db.executeQuery(SQL)
            If dt.Rows.Count > 0 Then
                warningId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
            End If
            '
            If warningId <> 0 Then
                '
                ' increment count for matching warning
                '
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" & cpcore.db.encodeSQLDate(Now()) & " where id=" & warningId
                Call cpcore.db.executeQuery(SQL)
            Else
                '
                ' insert new record
                '
                CS = cpcore.db.csInsertRecord("Site Warnings", 0)
                If cpcore.db.csOk(CS) Then
                    Call cpcore.db.csSet(CS, "name", Name)
                    Call cpcore.db.csSet(CS, "description", Description)
                    Call cpcore.db.csSet(CS, "generalKey", generalKey)
                    Call cpcore.db.csSet(CS, "specificKey", specificKey)
                    Call cpcore.db.csSet(CS, "count", 1)
                    Call cpcore.db.csSet(CS, "DateLastReported", Now())
                    If True Then
                        Call cpcore.db.csSet(CS, "shortDescription", shortDescription)
                        Call cpcore.db.csSet(CS, "location", location)
                        Call cpcore.db.csSet(CS, "pageId", PageID)
                    End If
                End If
                Call cpcore.db.csClose(CS)
            End If
            '
        End Sub
        '
        '====================================================================================================
        '
        Friend Shared Sub appendInstallLog(cpCore As coreClass, ByVal message As String)
            Console.WriteLine("install, " & message)
            appendLog(cpCore, message, "install")
        End Sub
    End Class
End Namespace