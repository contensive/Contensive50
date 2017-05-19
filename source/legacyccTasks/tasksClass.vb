
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers

Namespace Contensive.Core

    Public Class tasksClass
        Private cpCore As Contensive.Core.coreClass
        '
        Dim LogCheckDateLast As Date
        '
        '
        '
        Public Sub Process()
            '
            'On Error GoTo ErrorTrap
            '
            'Dim ResultMessage As String = ""
            'Dim SQLDateNow As String
            'Dim CS as integer
            'Dim SQL As String
            'Dim Filename As String
            'Dim DataSource As String
            'Dim CSVFilename As String
            'Dim NotifyEMail As String = ""
            'Dim NotifyBody As String
            'Dim NotifySubject As String
            'Dim SQLFieldName As String
            'Dim PreviousProcessAborted As Boolean
            ''
            'Dim cp As CPClass
            'Dim appStatus as integer
            'Dim appName As String
            ''
            'appName = ""
            'SQLDateNow = app.EncodeSQLDate(Now)
            Throw New NotImplementedException
            'For Each AppService In KernelService.AppServices
            '    appName = cpCore.app.Name
            '    appStatus = cpCore.app.Status
            '    If appStatus = applicationStatusEnum.ApplicationStatusRunning Then
            '        '
            '        cp = New CPClass(appName)
            '        If Not (cp Is Nothing) Then
            '            Call cp.initForProcess(appName)
            '            cmc = cp.getCmcHack()
            '            If true Then
            '                '
            '                '   Check this server for anything in the tasks queue
            '                '
            '                If cmc.csv_IsContentFieldSupported("Tasks", "ID") Then
            '                    CS = cmc.cpCore.app.csOpen("Tasks", "DateCompleted is null")
            '                    Do While cmc.cpCore.app.csv_IsCSOK(CS)
            '                        PreviousProcessAborted = False
            '                        If (true) Then
            '                            '
            '                            ' Since only one ccTask can run at a time, if a task is found to be started, it must have aborted.
            '                            ' Mark it aborted so it will not lock up all task processes
            '                            '
            '                            PreviousProcessAborted = (cmc.csv_cs_getDate(CS, "DateStarted") <> Date.MinValue)
            '                        End If
            '                        If PreviousProcessAborted Then
            '                            Call cmc.cpCore.app.csv_SetCS(CS, "DateCompleted", Now)
            '                            Call cmc.cpCore.app.csv_SetCS(CS, "ResultMessage", "This task failed to complete.")
            '                        Else
            '                            If cmc.csv_IsContentFieldSupported("Tasks", "NotifyEmail") Then
            '                                NotifyEMail = cmc.csv_cs_get(CS, "NotifyEmail")
            '                            End If
            '                            NotifyBody = ""
            '                            If cmc.csv_GetSiteProperty("BuildVersion", "", SystemMemberID) >= "3.3.583" Then
            '                                SQLFieldName = "SQLQuery"
            '                            Else
            '                                SQLFieldName = "SQL"
            '                            End If
            '                            Select Case genericController.vbUCase(cmc.cpCore.app.csv_cs_getText(CS, "Command"))
            '                                Case "BUILDCSV"
            '                                    '
            '                                    ' Build CSV
            '                                    '
            '                                    DataSource = cmc.cpCore.app.csv_cs_getText(CS, "DataSource")
            '                                    SQL = cmc.cpCore.app.csv_cs_getText(CS, SQLFieldName)
            '                                    Filename = genericController.vbReplace(cmc.cpCore.app.csv_cs_getText(CS, "Filename"), "/", "\")
            '                                    ResultMessage = BuildCSV(cmc, DataSource, SQL, cmc.cpCore.app.config.physicalFilePath & Filename)
            '                                    If ResultMessage <> "" Then
            '                                        NotifyBody = "This email is to notify you that there was a problem with your export data [" & ResultMessage & "]  on [" & cmc.appEnvironment.name & "]"
            '                                        NotifySubject = "There was a problem with your export"
            '                                    Else
            '                                        NotifyBody = "This email is to notify you that your export is ready on [" & cmc.appEnvironment.name & "]"
            '                                        NotifySubject = "Export is ready"
            '                                    End If
            '                                Case "BUILDXML"
            '                                    '
            '                                    ' Build XML
            '                                    '
            '                                    DataSource = cmc.cpCore.app.csv_cs_getText(CS, "DataSource")
            '                                    SQL = cmc.cpCore.app.csv_cs_getText(CS, SQLFieldName)
            '                                    Filename = genericController.vbReplace(cmc.cpCore.app.csv_cs_getText(CS, "Filename"), "/", "\")
            '                                    ResultMessage = BuildXML(cmc, DataSource, SQL, cmc.cpCore.app.config.physicalFilePath & Filename)
            '                                    NotifyBody = "This email is to notify you that your XML export is ready on [" & cmc.appEnvironment.name & "]"
            '                                    NotifySubject = "XML export is ready"
            '                                Case "IMPORTCSV"
            '                                    '
            '                                    ' moved to aoImportWizard
            '                                    '
            '                                    Call cmc.csv_ExecuteAddon(0, "{5254FAC6-A7A6-4199-8599-0777CC014A13}", "", addonContextEnum.ContextSimple, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "")
            '                                    ''
            '                                    '' Import a CSV file
            '                                    ''
            '                                    'Set ImportProcessor = New ProcessImportClass
            '                                    ''
            '                                    'CSVFilename = genericController.vbReplace(cmc.cpCore.app.csv_cs_getText(CS, "Filename"), "/", "\")
            '                                    'ImportMapFilename = genericController.vbReplace(cmc.cpCore.app.csv_cs_getText(CS, "ImportMapFilename"), "/", "\")
            '                                    ''
            '                                    'ResultMessage = ImportProcessor.ProcessCSV(cmc, CSVFilename, ImportMapFilename)
            '                                    'If ResultMessage <> "" Then
            '                                    '    NotifyBody = "This email is to notify you that your data import is complete for [" & cmc.appEnvironment.name & "]" & vbCrLf & "The following errors occurred during import" & vbCrLf & ResultMessage
            '                                    'Else
            '                                    '    NotifyBody = "This email is to notify you that your data import is complete for [" & cmc.appEnvironment.name & "]"
            '                                    'End If
            '                                    'NotifySubject = "Import Completed"
            '                                Case Else
            '                            End Select
            '                            Call cmc.cpCore.app.csv_SetCS(CS, "DateCompleted", Now)
            '                            If true Then
            '                                If ResultMessage = "" Then
            '                                    ResultMessage = "ok"
            '                                End If
            '                                Call cmc.cpCore.app.csv_SetCS(CS, "ResultMessage", ResultMessage)
            '                            End If
            '                            If NotifyEMail <> "" And NotifyBody <> "" Then
            '                                Call cmc.csv_SendEmail(NotifyEMail, cmc.csv_GetSiteProperty("EmailFromAddress", "", 0), "Task Completion Notification", NotifyBody)
            '                            End If
            '                        End If
            '                        cmc.cpCore.app.csv_NextCSRecord(CS)
            '                    Loop
            '                    Call cmc.cpCore.app.csv_CloseCS(CS)
            '                End If
            '            End If
            '            cmc = Nothing
            '        End If
            '        Call cp.Dispose()
            '        cp = Nothing
            '    End If
            'Next
            'AppService = Nothing
            'KernelService = Nothing
            '
            '            Exit Sub
            'ErrorTrap:
            '            Call HandleClassTrapErrorResumeNext("Process", "trap", appName)
            '            Err.Clear()
        End Sub
        '
        '==========================================================================================
        '   Task Processes
        '       reurns empty if OK
        '==========================================================================================
        '
        Private Function BuildCSV(ByVal DataSource As String, ByVal SQL As String, ByVal Filename As String) As String
            BuildCSV = ""
            On Error GoTo ErrorTrap
            '
            'Dim fs As New fileSystemClass
            Dim CS as integer
            Dim Delimiter As String
            Dim FieldName As String
            Dim Copy As String
            Dim FieldNames() As String
            Dim FieldNameCnt as integer
            Dim FieldNamePtr as integer
            Dim FieldNameSize as integer
            Dim RowBuffer As String = ""
            Dim appName As String
            '
            appName = cpCore.serverconfig.appConfig.name
            CS = cpCore.db.cs_openCsSql_rev(DataSource, SQL)
            If cpCore.db.cs_ok(CS) Then
                '
                ' ----- print out the field names
                '
                'Delimiter = ""
                FieldNameCnt = 0
                FieldNameSize = 100
                ReDim FieldNames(FieldNameSize)
                FieldName = cpCore.db.cs_getFirstFieldName(CS)
                Do While (FieldName <> "")
                    If FieldNameCnt > FieldNameSize Then
                        FieldNameSize = FieldNameSize + 10
                        ReDim Preserve FieldNames(FieldNameSize)
                    End If
                    FieldNames(FieldNameCnt) = FieldName
                    RowBuffer = RowBuffer & ",""" & FieldName & """"
                    FieldName = cpCore.db.cs_getNextFieldName(CS)
                    FieldNameCnt = FieldNameCnt + 1
                    'DoEvents()
                Loop
                ReDim Preserve FieldNames(FieldNameCnt - 1)
                If RowBuffer <> "" Then
                    Call cpCore.cdnFiles.appendFile(Filename, Mid(RowBuffer, 2) & vbCrLf)
                    '
                    ' ----- print out the values
                    '
                    Do While cpCore.db.cs_ok(CS)
                        Delimiter = ""
                        RowBuffer = ""
                        For FieldNamePtr = 0 To FieldNameCnt - 1
                            Copy = cpCore.db.cs_get(CS, FieldNames(FieldNamePtr))
                            Copy = genericController.vbReplace(Copy, """", """""")
                            ' if propertly quoted, line breaks can be preserved
                            'Copy = genericController.vbReplace(Copy, vbCrLf, " ")
                            'Copy = genericController.vbReplace(Copy, vbCr, " ")
                            'Copy = genericController.vbReplace(Copy, vbLf, " ")
                            RowBuffer = RowBuffer & ",""" & Copy & """"
                            'DoEvents()
                        Next
                        Call cpCore.cdnFiles.appendFile(Filename, Mid(RowBuffer, 2) & vbCrLf)
                        Call cpCore.db.cs_goNext(CS)
                        'DoEvents()
                    Loop
                End If
            End If
            '
            Call cpCore.db.cs_Close(CS)
            '
            Exit Function
ErrorTrap:
            BuildCSV = "export failed, " & genericController.GetErrString(Err)
            Call HandleClassTrapErrorResumeNext("BuildCSV", BuildCSV, appName)
            Err.Clear()
        End Function
        '
        '==========================================================================================
        '   Task Processes
        '==========================================================================================
        '
        Private Function BuildXML(ByVal DataSource As String, ByVal SQL As String, ByVal Filename As String) As String
            BuildXML = ""
            On Error GoTo ErrorTrap
            '
            'Dim fs As New fileSystemClass
            Dim CS As Integer
            Dim Delimiter As String
            Dim FieldName As String
            Dim Copy As String
            Dim FieldNames() As String
            Dim FieldNameCnt As Integer
            Dim FieldNamePtr As Integer
            Dim FieldNameSize As Integer
            Dim RowBuffer As String
            Dim appName As String
            '
            appName = cpCore.serverconfig.appConfig.name
            CS = cpCore.db.cs_openCsSql_rev(DataSource, SQL)
            If cpCore.db.cs_ok(CS) Then
                '
                ' ----- setup the field names
                '
                FieldNameCnt = 0
                FieldNameSize = 100
                ReDim FieldNames(FieldNameSize)
                FieldName = cpCore.db.cs_getFirstFieldName(CS)
                Do While (FieldName <> "")
                    If FieldNameCnt > FieldNameSize Then
                        FieldNameSize = FieldNameSize + 10
                        ReDim Preserve FieldNames(FieldNameSize)
                    End If
                    FieldNames(FieldNameCnt) = FieldName
                    FieldName = cpCore.db.cs_getNextFieldName(CS)
                    FieldNameCnt = FieldNameCnt + 1
                    'DoEvents()
                Loop
                FieldNameCnt = FieldNameCnt - 1
                ReDim Preserve FieldNames(FieldNameCnt)
                If FieldNameCnt > 0 Then
                    Call cpCore.cdnFiles.appendFile(Filename, "<Content>" & vbCrLf)
                    '
                    ' ----- print out the values
                    '
                    Do While cpCore.db.cs_ok(CS)
                        Delimiter = ""
                        RowBuffer = "<Record>"
                        For FieldNamePtr = 0 To FieldNameCnt - 1
                            FieldName = FieldNames(FieldNamePtr)
                            If FieldName <> "" Then
                                Copy = cpCore.db.cs_get(CS, FieldNames(FieldNamePtr))
                                Copy = cpcore.htmlDoc.html_EncodeHTML(Copy)
                                If Copy = "" Then
                                    RowBuffer = RowBuffer & "<" & FieldName & " />"
                                Else
                                    RowBuffer = RowBuffer & "<" & FieldName & ">" & Copy & "</" & FieldName & ">"
                                End If
                            End If
                            'DoEvents()
                        Next
                        RowBuffer = RowBuffer & "</Record>"
                        Call cpCore.cdnFiles.appendFile(Filename, RowBuffer & vbCrLf)
                        Call cpCore.db.cs_goNext(CS)
                        'DoEvents()
                    Loop
                    Call cpCore.cdnFiles.appendFile(Filename, "</Content>" & vbCrLf)
                End If
            End If
            '
            Call cpCore.db.cs_Close(CS)
            'FS = Nothing
            '
            Exit Function
ErrorTrap:
            BuildXML = "export failed, " & genericController.GetErrString(Err)
            Call HandleClassTrapErrorResumeNext("BuildXML", BuildXML, appName)
            Err.Clear()
        End Function
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Private Sub HandleClassTrapErrorResumeNext(ByVal MethodName As String, ByVal Context As String, ByVal appName As String)
            '
            ' ----- Append to the Content Server Log File
            '
            cpCore.handleLegacyError3(appName, Context, "ccTasks", "TasksClass", MethodName, Err.Number, Err.Source, Err.Description, True, True, "")
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        Public Sub New(cpCore As Contensive.Core.coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
    End Class
End Namespace
