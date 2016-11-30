
Imports Contensive.Core
'Imports Contensive.Core
Imports Contensive.Core.ccCommonModule
'Imports System.messaging

'Imports Interop.adodb
Imports System.Xml

Namespace Contensive.Core
    Module cmdModule
        '
        '===================================================================================
        ' Cmd
        '
        '   Executes commands from the command line.
        '   Eventually this could be used as a model for the message queueing service
        '
        '===================================================================================
        '
        'Private cp As CPClass
        '
        Public Sub Main()
            '
            Dim cmd As String
            Dim MonitorPort as integer
            Dim Tracker As New cmdTrackClass
            Dim maxInstanceCnt as integer
            Dim cmdList As New List(Of String)
            Dim cmds As String()
            Dim cp As CPClass
            'Dim cpCore As cpCoreClass
            '
            ' put authtooken in a config file or make it a command line argument
            '
            cp = New CPClass()
            cp.core.appendLog("cmdModule.main, enter")
            '
            If Not cp.clusterOk Then
                '
                ' some kind of error to handle
                '
            Else
                '
                ' cluster ok, process the command
                '
                MonitorPort = EncodeInteger(getCmdLineValue(cp.core, "port", My.Application.CommandLineArgs, "0"))
                maxInstanceCnt = EncodeInteger(getCmdLineValue(cp.core, "max", My.Application.CommandLineArgs, "5"))
                '
                cp.core.appendLog("cmdModule.main, MonitorPort=[" & MonitorPort & "], maxInstanceCnt=[" & maxInstanceCnt & "]")
                '
                If Not Tracker.AllowInstance(maxInstanceCnt) Then
                    '
                    ' do not allow this instance 
                    '
                    cp.core.appendLog("command blocked because too many instances running")
                Else
                    If MonitorPort = 0 Then
                        '
                        ' if command line has no port, use the command argument as the ServerCmd
                        '
                        'cmd = ""
                        'For Each arg As String In My.Application.CommandLineArgs
                        '    cmd &= " " & arg
                        'Next
                        'If cmd <> "" Then
                        '    cmd = cmd.Substring(1)
                        'End If
                        '
                        cp.core.appendLog("cmdModule.main, run command line arguments since monitor port = 0")
                        '
                        Call ExecuteCmd(cp.core, My.Application.CommandLineArgs)
                    Else
                        '
                        ' local Mode - Execute out of the server command queue
                        '
                        '
                        cp.core.appendLog("cmdModule.main, call getNextAsyncCmd and loop until command is 'error', or empty ")
                        '
                        cmd = GetNextAsyncCmd(cp.core, "127.0.0.1", MonitorPort, "", "")
                        Do While cmd <> "" And InStr(1, cmd, "error", vbTextCompare) <> 1
                            '
                            cp.core.appendLog("cmdModule.main, looping, cmd=[" & cmd & "]")
                            '
                            If cmd <> "" Then
                                cmds = ccCommonModule.SplitDelimited(cmd, " ")
                                For Each arg As String In cmds
                                    '
                                    cp.core.appendLog("cmdModule.main, convert cmd to list, arg=[" & arg & "]")
                                    '
                                    Call cmdList.Add(arg)
                                Next
                                Dim tmp As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = cmdList.AsReadOnly
                                '
                                cp.core.appendLog("cmdModule.main, now execute full cmd, tmp=[" & tmp.ToString & "]")
                                '
                                Call ExecuteCmd(cp.core, tmp)
                            End If
                            cmd = GetNextAsyncCmd(cp.core, "127.0.0.1", MonitorPort, "", "")
                        Loop
                    End If
                End If
                Tracker = Nothing
            End If
        End Sub
        '
        '
        '
        Private Function GetNextAsyncCmd(cpCore As cpCoreClass, ByVal IPAddress As String, ByVal port as integer, ByVal Username As String, ByVal Password As String) As String
            On Error GoTo ErrorTrap
            '
            'Dim runAtServer As New runAtServerClass( cpcore )
            Dim runAtServer As New runAtServerClass(cpCore)
            Dim ServerResponse As String
            Dim StatusLine As String
            Dim hint As String
            Dim throwAwayFirstLine As String
            '
            cpCore.appendLog("cmdMethod.getNextAsyncCmd enter")
            '
            'hint = "setting runAtServer properties"
            runAtServer.ipAddress = IPAddress
            runAtServer.port = port
            runAtServer.username = Username
            runAtServer.password = Password
            runAtServer.userAgent = "Cmd " & Threading.Thread.CurrentThread.ManagedThreadId
            'hint = "callng runAtServer executeCmd"
            GetNextAsyncCmd = runAtServer.executeCmd("GetNextAsyncCmd", "")
            'hint = "determining return status"
            cpCore.appendLog("cmdMethod.getNextAsyncCmd exit, status=Mid(GetNextAsyncCmd, 1, 2)=[" & Mid(GetNextAsyncCmd, 1, 2) & "]")
            If Mid(GetNextAsyncCmd, 1, 2) <> "OK" Then
                '
                ' There was a problem
                '
                cpCore.appendLog("cmdMethod.getNextAsyncCmd exit, NOT OK")
                GetNextAsyncCmd = GetNextAsyncCmd
            Else
                '
                ' Command returned, trim off OK and execute
                '
                throwAwayFirstLine = getLine(GetNextAsyncCmd)
            End If
            '
            cpCore.appendLog("cmdMethod.getNextAsyncCmd exit, return  [" & GetNextAsyncCmd & "]")
            '
            Exit Function
ErrorTrap:
            cpCore.handleLegacyError3("(unknown)", "errorTrap in ccCmd", "App.EXEName", "cmdModule", "GetNextAsyncCmd", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Function
        '
        ' Execute Server Command
        '
        '   Format:
        '       Start AppName=MyApp arg2=value2 ...
        '
        Private Sub ExecuteCmd(cpCore As cpCoreClass, ByVal cmdLine As System.Collections.ObjectModel.ReadOnlyCollection(Of String))
            Try
                '
                Dim hint As String
                Dim Optionstring As String
                Dim AddonIdGuidOrName As String
                Dim ArchiveFilename As String
                Dim AddFilename As String
                Dim Filename As String
                Dim SiteBuilder As builderClass
                Dim Method As String
                Dim AppName As String
                Dim LoopPtr as integer
                LoopPtr = 0
                '
                cpCore.appendLog("cmdModule.executeCmd, enter. cmdLine.count=" & cmdLine.Count)
                '
                'hint = "enter"
                If (cmdLine.Count > 0) Then
                    LoopPtr = 0
                    Method = cmdLine(0)
                    AppName = getCmdLineValue(cpCore, "appname", cmdLine)
                    If (AppName = "") Then
                        '
                        '
                        '
                        Throw New ArgumentException("Async command [" & Method & "] called and applicationname is blank")
                    Else
                        '20151109 - removed but unsure what in initContext can/must be moved to cp constructor
                        'cp.execute_initContext()
                        'If (cpCore.app.status <> applicationStatusEnum.ApplicationStatusReady) Then
                        'Else

                        'End If
                        Select Case UCase(Method)
                            Case "INSTALLADDON", "INSTALLADDONS"
                                '
                                ' Install Add-ons in the AddonInstall folder of the applications virtual folder
                                '
                                Call InstallAddons(cpCore, cpCore.app.dataBuildVersion)
                            Case "IISRESET"
                                '
                                ''Call appendErrorLog("ccCmd CmdModule.ExecuteServerCmd.IISRESET")
                                ' try 3 seconds of sleep - this gives a chance for a page that calls this
                                ' command to get away and created the waiting popup
                                '
                                System.Threading.Thread.Sleep(3000)
                                SiteBuilder = New builderClass(cpCore)
                                Call SiteBuilder.web_reset()
                            Case "UNZIPANDDELETEFILE"
                                '
                                '
                                '
                                Filename = getCmdLineArguments(cpCore, cmdLine)
                                'SiteBuilder = New siteBuilderclass(cpcore)
                                Call cpCore.app.privateFiles.UnzipFile(Filename)
                                Call cpCore.app.privateFiles.DeleteFile(Filename)
                            Case "UNZIPFILE"
                                '
                                '
                                '
                                Filename = getCmdLineArguments(cpCore, cmdLine)
                                Call cpCore.app.privateFiles.UnzipFile(Filename)
                            Case "ZIPFILE"
                                '
                                '
                                '
                                ArchiveFilename = DecodeResponseVariable(getCmdLineValue(cpCore, "archive", cmdLine, "", " "))
                                AddFilename = DecodeResponseVariable(getCmdLineValue(cpCore, "add", cmdLine, "", " "))
                                'SiteBuilder = New siteBuilderclass(cpcore)
                                Call cpCore.app.privateFiles.zipFile(ArchiveFilename, AddFilename)
                                'Case "REGISTERDOTNET"
                                '    '
                                '    '
                                '    '
                                '    Filename = getCmdLineArguments(cpCore, cmdLine)
                                '    SiteBuilder = New siteBuilderClass(cpCore)
                                '    Call SiteBuilder.RegisterDotNet(Filename)
                                'Case "ADDSITE"
                                '    '
                                '    ' Add a new site
                                '    '   leave cmc here bc the only purpose is to verify appName is not used
                                '    '
                                '    'hint = hint & ",begin AddSite"
                                '    cp = New CPClass
                                '    'hint = hint & ",100"
                                '    If Not (cp Is Nothing) Then
                                '        Call cp.init(AppName)
                                '        'hint = hint & ",110"
                                '        cmc = cp.getCmcHack()
                                '        'hint = hint & ",120"
                                '        appNameAvailable = (cmc.app.status = applicationStatusEnum.ApplicationStatusNotFound)
                                '        cmc = Nothing
                                '    End If
                                '    Call cp.Dispose()
                                '    cp = Nothing
                                '    'hint = hint & ",130"
                                '    ''
                                '    'Set cmc = New cpCoreClass
                                '    'If true Then
                                '    '    CSConnection = cmc.csv_OpenConnection(AppName)
                                '    '    appNameAvailable = (CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusNotFound)
                                '    '    Call cmc.csv_CloseConnection(CSConnection.ConnectionHandle)
                                '    'End If
                                '    'Set cmc = Nothing
                                '    'hint = hint & ",begin AddSite"
                                '    If appNameAvailable Then
                                '        IPAddress = getCmdLineValue("IPAddress", cmdLine, "", " ")
                                '        Domainname = getCmdLineValue("Domainname", cmdLine, "", " ")
                                '        DbPath = getCmdLineValue("DbPath", cmdLine, "", " ")
                                '        ContentFilesPath = getCmdLineValue("ContentFilesPath", cmdLine, "", " ")
                                '        wwwRootPath = getCmdLineValue("wwwRootPath", cmdLine, "", " ")
                                '        SMTPServer = getCmdLineValue("SMTPServer", cmdLine, "", " ")
                                '        AdminEmail = getCmdLineValue("AdminEmail", cmdLine, "", " ")
                                '        siteKey = getCmdLineValue("siteKey", cmdLine, "", " ")
                                '        DefaultDoc = getCmdLineValue("defaultDoc", cmdLine, "index.php", " ")
                                '        SiteType = EncodeInteger(getCmdLineValue("siteType", cmdLine, SiteTypeBasePhp, " "))
                                '        addonList = getCmdLineValue("addonList", cmdLine, "", " ")
                                '        '
                                '        'hint = hint & ",140"
                                '        SiteBuilder = New siteBuilderClass
                                '        'hint = hint & ",150"
                                '        If (Not SiteBuilder Is Nothing) Then
                                '            Call SiteBuilder.CreateNewSite(AppName, IPAddress, Domainname, DbPath, ContentFilesPath, DsnPath, True, wwwRootPath, DefaultDoc, SiteType, SMTPServer, AdminEmail)
                                '            'hint = hint & ",160"
                                '            '
                                '            cp = New CPClass
                                '            'hint = hint & ",170"
                                '            If Not (cp Is Nothing) Then
                                '                Call cp.init(AppName)
                                '                If (siteKey <> "") Then
                                '                    Call cp.Site.SetProperty("siteKey", siteKey)
                                '                    'CSConnection = cmc.csv_OpenConnection(AppName)
                                '                    'If CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusRunning Then
                                '                    '    Call cmc.app.setSiteProperty("siteKey", siteKey, 0)
                                '                    'End If
                                '                End If
                                '                '
                                '                If (addonList <> "") Then
                                '                    addons = Split(addonList, ",")
                                '                    For Ptr = 0 To UBound(addons)
                                '                        Call AddonInstall.UpgradeAllAppsFromLibCollection(addons(Ptr), AppName, IISResetRequired, RegisterList, ErrorMessage, False)
                                '                    Next
                                '                    If RegisterList <> "" Then
                                '                        Files = Split(RegisterList, ",")
                                '                        For Ptr = 0 To UBound(Files)
                                '                            If Files(Ptr) <> "" Then
                                '                                Call SiteBuilder.RegisterActiveX(Files(Ptr))
                                '                                Call SiteBuilder.RegisterDotNet(Files(Ptr))
                                '                            End If
                                '                        Next
                                '                    End If
                                '                    If IISResetRequired Then
                                '                        Call SiteBuilder.ResetIIS()
                                '                    End If
                                '                End If
                                '            End If
                                '            Call cp.Dispose()
                                '            cp = Nothing
                                '        End If
                                '        SiteBuilder = Nothing
                                '    End If
                                '    'hint = hint & ",999"
                                '    '
                                '    ' 2009/06/06 - changed the createnewsite to start with upgrade, so core is added with the base during upgrade
                                '    '
                                '    ''
                                '    '' install core collection
                                '    ''
                                '    'Call AddonInstall.UpgradeLibCollection2("{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}", AppName, IISResetRequired, RegisterList, ErrorMessage)
                                '    'If ErrorMessage <> "" Then
                                '    'End If
                                '    'If RegisterList <> "" Then
                                '    '    Files = Split(RegisterList, ",")
                                '    '    For Ptr = 0 To UBound(Files)
                                '    '        If Files(Ptr) <> "" Then
                                '    '            Call SiteBuilder.RegisterActiveX(Files(Ptr))
                                '    '            Call SiteBuilder.RegisterDotNet(Files(Ptr))
                                '    '        End If
                                '    '    Next
                                '    'End If
                                '    'If IISResetRequired Then
                                '    '    Call SiteBuilder.ResetIIS
                                '    'End If
                                'Case "IMPORTSITE"
                                '    '
                                '    ' Import a site
                                '    '
                                '    cp = New CPClass
                                '    If Not (cp Is Nothing) Then
                                '        Call cp.init(AppName)
                                '        cmc = cp.getCmcHack()
                                '        appNameAvailable = (cmc.app.status = applicationStatusEnum.ApplicationStatusNotFound)
                                '        cmc = Nothing
                                '    End If
                                '    Call cp.Dispose()
                                '    cp = Nothing
                                '    'Set cmc = New cpCoreClass
                                '    'If true Then
                                '    '    CSConnection = cmc.csv_OpenConnection(AppName)
                                '    '    appNameAvailable = (CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusNotFound)
                                '    '    Call cmc.csv_CloseConnection(CSConnection.ConnectionHandle)
                                '    'End If
                                '    'Set cmc = Nothing
                                '    If appNameAvailable Then
                                '        IPAddress = getCmdLineValue("IPAddress", cmdLine, "", " ")
                                '        Domainname = getCmdLineValue("Domainname", cmdLine, "", " ")
                                '        ODBCConnectionString = getCmdLineValue("ODBCConnectionString", cmdLine, "", " ")
                                '        ContentFilesPath = getCmdLineValue("ContentFilesPath", cmdLine, "", " ")
                                '        wwwRootPath = getCmdLineValue("wwwRootPath", cmdLine, "", " ")
                                '        SMTPServer = getCmdLineValue("SMTPServer", cmdLine, "", " ")
                                '        AdminEmail = getCmdLineValue("AdminEmail", cmdLine, "", " ")
                                '        siteKey = getCmdLineValue("siteKey", cmdLine, "", " ")
                                '        DefaultDoc = getCmdLineValue("defaultDoc", cmdLine, "index.php", " ")
                                '        '
                                '        SiteBuilder = New siteBuilderClass
                                '        If (Not SiteBuilder Is Nothing) Then
                                '            Call SiteBuilder.ImportSite(AppName, IPAddress, Domainname, ODBCConnectionString, ContentFilesPath, wwwRootPath, DefaultDoc, SMTPServer, AdminEmail)
                                '            '
                                '            cp = New CPClass
                                '            If Not (cp Is Nothing) Then
                                '                Call cp.init(AppName)
                                '                If (siteKey <> "") Then
                                '                    Call cp.Site.SetProperty("siteKey", siteKey)
                                '                    'CSConnection = cmc.csv_OpenConnection(AppName)
                                '                    'If CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusRunning Then
                                '                    '    Call cmc.app.setSiteProperty("siteKey", siteKey, 0)
                                '                    'End If
                                '                End If
                                '            End If
                                '            cp = Nothing
                                '        End If
                                '        SiteBuilder = Nothing
                                '    End If
                                '    '                    Set cmc = New cpCoreClass
                                '    '                    If true Then
                                '    '                        CSConnection = cmc.csv_OpenConnection(AppName)
                                '    '                        If CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusNotFound Then
                                '    '                            IPAddress = getCmdLineValue("IPAddress", cmdLine, "", " ")
                                '    '                            Domainname = getCmdLineValue("Domainname", cmdLine, "", " ")
                                '    '                            ODBCConnectionString = getCmdLineValue("ODBCConnectionString", cmdLine, "", " ")
                                '    '                            ContentFilesPath = getCmdLineValue("ContentFilesPath", cmdLine, "", " ")
                                '    '                            wwwRootPath = getCmdLineValue("wwwRootPath", cmdLine, "", " ")
                                '    '                            SMTPServer = getCmdLineValue("SMTPServer", cmdLine, "", " ")
                                '    '                            AdminEmail = getCmdLineValue("AdminEmail", cmdLine, "", " ")
                                '    '                            siteKey = getCmdLineValue("siteKey", cmdLine, "", " ")
                                '    '                            DefaultDoc = getCmdLineValue("defaultDoc", cmdLine, "index.php", " ")
                                '    '                            '
                                '    '                            Set SiteBuilder = New SiteBuilderClass
                                '    '                            Call SiteBuilder.ImportSite(AppName, IPAddress, Domainname, ODBCConnectionString, ContentFilesPath, wwwRootPath, DefaultDoc, SMTPServer, AdminEmail)
                                '    '                            Set SiteBuilder = Nothing
                                '    '                            '
                                '    '                            If (siteKey <> "") Then
                                '    '                                CSConnection = cmc.csv_OpenConnection(AppName)
                                '    '                                If CSConnection.ApplicationStatus = applicationStatusEnum.ApplicationStatusRunning Then
                                '    '                                    Call cmc.app.setSiteProperty("siteKey", siteKey, 0)
                                '    '                                End If
                                '    '                            End If
                                '    '                        End If
                                '    '                        Call cmc.csv_CloseConnection(CSConnection.ConnectionHandle)
                                '    '                    End If
                                '    '                    Set cmc = Nothing
                                'Case "FIXROOTUSER"
                                '    '
                                '    ' Add a root user to the
                                '    '
                                '    cp = New CPClass
                                '    If Not (cp Is Nothing) Then
                                '        Call cp.init(AppName)
                                '        cmc = cp.getCmcHack()
                                '        If true Then
                                '            'CSConnection = cmc.csv_OpenConnection(AppName)
                                '            If cmc.app.status = applicationStatusEnum.ApplicationStatusRunning Then
                                '                cs = cmc.app.db_csOpen("people", "username='root'", , False)
                                '                If Not cmc.app.csv_IsCSOK(cs) Then
                                '                    Call cmc.app.csv_CloseCS(cs)
                                '                    cs = cmc.app.csv_InsertCSRecord("people", 0)
                                '                End If
                                '                Call cmc.app.csv_SetCS(cs, "name", "Root")
                                '                Call cmc.app.csv_SetCS(cs, "active", "1")
                                '                Call cmc.app.csv_SetCS(cs, "username", "root")
                                '                Call cmc.app.csv_SetCS(cs, "password", "contensive")
                                '                Call cmc.app.csv_SetCS(cs, "developer", "1")
                                '                Call cmc.app.csv_SaveCS(cs)
                                '                Call cmc.app.csv_NextCSRecord(cs)
                                '                Do While cmc.app.csv_IsCSOK(cs)
                                '                    Call cmc.app.csv_SetCS(cs, "active", 0)
                                '                    Call cmc.app.csv_NextCSRecord(cs)
                                '                Loop
                                '                Call cmc.app.csv_CloseCS(cs)
                                '            End If
                                '            'Call cmc.csv_CloseConnection(CSConnection.ConnectionHandle)
                                '        End If
                                '        cmc = Nothing
                                '    End If
                                '    Call cp.Dispose()
                                '    cp = Nothing
                            Case "RUNPROCESS"
                                '
                                ' execute process addon
                                '
                                If cpCore.cluster.ok Then
                                    '20151109 - removed but unsure what in initContext can/must be moved to cp constructor
                                    'Call cp.execute_initContext()
                                    If cpCore.app.status = applicationStatusEnum.ApplicationStatusReady Then
                                        AddonIdGuidOrName = EncodeInteger(getCmdLineValue(cpCore, "AddonId", cmdLine, "", " "))
                                        If AddonIdGuidOrName = "0" Then
                                            AddonIdGuidOrName = getCmdLineValue(cpCore, "addonguid", cmdLine, "", " ")
                                            If AddonIdGuidOrName = "" Then
                                                AddonIdGuidOrName = getCmdLineValue(cpCore, "addonname", cmdLine, "", " ")
                                            End If
                                        End If
                                        Optionstring = getCmdLineValue(cpCore, "optionstring", cmdLine, "", " ")
                                        '
                                        ' for now, there is nothing to return
                                        '
                                        Call cpCore.executeAddonAsProcess(AddonIdGuidOrName, Optionstring)
                                    End If
                                    'cmc = Nothing
                                End If
                                Call cpCore.dispose()
                                cpCore = Nothing
                                '
                                '                    Set cmc = New cpCoreClass
                                '                    If true Then
                                '                        CSConnection = cmc.csv_OpenConnection(AppName)
                                '                        If CSConnection.ApplicationStatus = ApplicationStatusRunning Then
                                '                            AddonIdGuidOrName = encodeInteger(getCmdLineValue("AddonId", cmdLine, "", " "))
                                '                            If AddonIdGuidOrName = "0" Then
                                '                                AddonIdGuidOrName = getCmdLineValue("addonguid", cmdLine, "", " ")
                                '                                If AddonIdGuidOrName = "" Then
                                '                                    AddonIdGuidOrName = getCmdLineValue("addonname", cmdLine, "", " ")
                                '                                End If
                                '                            End If
                                '                            Optionstring = getCmdLineValue("optionstring", cmdLine, "", " ")
                                '                            '
                                '                            ' for now, there is nothing to return
                                '                            '
                                '                            Call cmc.csv_ExecuteAddonAsProcess(AddonIdGuidOrName, Optionstring, Nothing, True)
                                '                        End If
                                '                        Call cmc.csv_CloseConnection(CSConnection.ConnectionHandle)
                                '                    End If
                                '                    Set cmc = Nothing
                            Case Else
                                '
                                ' Unknown
                                '
                                cpCore.handleLegacyError2("CmdModule", "ExecuteServerCmd", "Unknown command [" & UCase(Method) & "]")
                        End Select
                    End If
                End If
                '
                cpCore.appendLog("cmdModule.executeCmd, exit")
            Catch ex As Exception
                '
                ' cluster needs a log
                '
                Call cpCore.handleExceptionLegacyRow2(ex, "cmdModule", "executeCmd", "unexpected exception")
            End Try
        End Sub
        '
        '===================================================================================
        '
        '===================================================================================
        '
        Private Sub InstallAddons(cpCore As cpCoreClass, buildVersion As String)
            On Error GoTo ErrorTrap
            '
            Dim builder As New builderClass(cpCore)
            Dim Alarm As Integer
            '
            ' Wait 5 seconds -- to give the person requesting it a chance to get their page back
            ' eliminate this with a progress animation that monitors 'done' status for this operation
            ' and survives the iisreset.
            '
            Threading.Thread.Sleep(5000)
            '
            ' Go add the addons
            '
            If cpCore.app.status = applicationStatusEnum.ApplicationStatusReady Then
                Call builder.InstallAddons(False, buildVersion)
            End If
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3(cpCore.app.config.name, "errorTrap in ccCmd", "App.EXEName", "cmdModule", "InstallAddons", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '
        '
        Private Sub appendCmdLog(cpCore As cpCoreClass, ByVal message As String)
            On Error GoTo ErrorTrap
            '
            cpCore.appendLog(Replace(Replace(message, vbCr, ""), vbLf, ""), "process", "cmd")
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3("(unknown)", "errorTrap in ccCmd", "App.EXEName", "cmdModule", "appendCmdLog", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '
        '
        Private Function getCmdLineValue(cpCore As cpCoreClass, ByVal cmdLineName As String, ByVal cmdLine As System.Collections.ObjectModel.ReadOnlyCollection(Of String), Optional ByVal defaultValue As String = "", Optional ByVal ignoreMe As String = "") As String
            Dim returnValue As String = ""
            Try
                '
                'cp.core.AppendLog("cmdModule.getCmdLineValue, cmdLineName=" & cmdLine.Count & ", cmdLine=[" & cmdLine.ToString & "], defaultValue=[" & defaultValue & "]")
                '
                For Each arg As String In cmdLine
                    'cp.core.AppendLog("cmdModule.getCmdLineValue, testing arg [" & arg & "]")
                    returnValue = DecodeResponseVariable(getSimpleNameValue(cmdLineName, arg, "", " "))
                    If returnValue <> "" Then
                        Exit For
                    End If
                Next
                If returnValue = "" Then
                    returnValue = defaultValue
                End If
                '
                'cp.core.AppendLog("cmdModule.getCmdLineValue[" & cmdLineName & "], returned [" & returnValue & "]")
                '
            Catch ex As Exception
                cpCore.handleLegacyError3("(unknown)", "Eception in cmdModule.getCmdLineValue", "App.EXEName", "cmdModule", "appendCmdLog", 0, "ccCmd", ex.ToString, True, True, "")
            End Try
            Return returnValue
        End Function
        '
        '
        '
        Private Function getCmdLineArguments(cpCore As cpCoreClass, ByVal cmdLine As System.Collections.ObjectModel.ReadOnlyCollection(Of String)) As String
            Dim returnValue As String = ""
            Dim isFirst = True
            Try
                If cmdLine.Count > 1 Then
                    If isFirst Then
                        isFirst = False
                    Else
                        For Each arg As String In cmdLine
                            returnValue = " " & DecodeResponseVariable(arg)
                        Next
                        returnValue = returnValue.Substring(1)
                    End If
                End If
                '
                cpCore.appendLog("cmdModule.getCmdLineArguments, returned [" & returnValue & "]")
                '
            Catch ex As Exception
                cpCore.handleLegacyError3("(unknown)", "Eception in cmdModule.getCmdLineArguments", "App.EXEName", "cmdModule", "appendCmdLog", 0, "ccCmd", ex.ToString, True, True, "")
            End Try
            Return returnValue
        End Function

    End Module
End Namespace