
'Imports Contensive.Core
Imports Contensive.Core
Imports System.Xml

Namespace Contensive.Core
    Public Class houseKeepClass
        '
        '
        '
        Private cp As cpclass
        Dim LogCheckDateLast As Date
        '
        '
        '
        Public Sub HouseKeep(ByVal DebugMode As Boolean)
            '
            On Error GoTo ErrorTrap
            ''
            Dim EmailDropArchiveAgeDays As Integer
            Dim Pos As Integer
            Dim DomainNameList As String
            Dim DomainNamePrimary As String
            Dim MissingDatePtr As Integer
            Dim DateNumberWorking As Integer
            Dim OldestVisitSummaryWeCareAbout As Date
            Dim DefaultMemberName As String
            Dim PeopleCID As Integer
            Dim IISResetRequired As Boolean
            Dim RegisterList As String = ""
            Dim ArchiveDeleteNoCookie As Boolean
            Dim Content As String
            Dim ServerHousekeepTime As Date
            Dim ErrorMessage As String
            Dim OldestSummarizedDateNumber As Double
            Dim Yesterday As Date
            Dim LastTimeSummaryWasRun As Date
            Dim NextSummaryStartDate As Date
            Dim RightNowHour As Date
            Dim ALittleWhileAgo As Date
            Dim PeriodStart As Date
            Dim PeriodDatePtr As Double
            Dim PeriodStep As Double
            Dim StartOfHour As Date
            Dim DateNumber As Integer
            Dim TimeNumber As Integer
            Dim SumStartTime As Double
            Dim SumStopTime As Double
            Dim HoursPerDay As Integer
            Dim DateStart As Date
            Dim DateEnd As Date
            Dim NewVisitorVisits As Integer
            Dim SinglePageVisits As Integer
            Dim AuthenticatedVisits As Integer
            Dim NoCookieVisits As Integer
            Dim AveTimeOnSite As Double
            Dim PagesViewed As Integer
            Dim VisitCnt As Integer
            Dim OldestDateAdded As Date
            Dim emptyData As Object
            Dim NeedToClearCache As Boolean
            Dim ArchiveParentID As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim LoopPtr As Integer
            Dim Ptr As Integer
            Dim LocalFile As String
            Dim LocalFilename As String
            Dim Folders() As String
            Dim FolderCnt As Integer
            Dim CollectionGUID As String
            Dim CollectionName As String
            Dim LastChangeDate As String
            Dim SubFolderList As String
            Dim SubFolders() As String
            Dim SubFolder As String
            Dim Cnt As Integer
            Dim LocalGUID As String
            Dim LocalLastChangeDateStr As String
            Dim LocalLastChangeDate As Date
            Dim LibGUID As String
            Dim LibLastChangeDateStr As String
            Dim LibLastChangeDate As Date
            Dim LibListNode As xmlNode
            Dim LocalListNode As xmlNode
            Dim CollectionNode As xmlNode
            Dim LibraryCollections As New XmlDocument
            Dim LocalCollections As New XmlDocument
            Dim Doc As New XmlDocument
            ''Dim AppService As appServicesClass
            Dim cpCore As coreClass
            '   Dim CSConnection As appEnvironmentStruc
            Dim AlarmTimeString As String
            Dim AlarmTimeMinutesSinceMidnight As Double
            Dim LogDate As Date
            Dim FolderName As String
            Dim FileList As String
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim FileSplit() As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            'Dim fs As New fileSystemClass
            Dim VisitArchiveAgeDays As Integer
            Dim GuestArchiveAgeDays As Integer
            Dim VisitArchiveDate As Date
            Dim RunServerHousekeep As Boolean
            Dim NewHour As Boolean
            '
            Dim rightNow As Date
            Dim LastCheckDateTime As Date
            '
            Dim LastCheckMinutesFromMidnight As Double
            Dim minutesSinceMidnight As Double
            '
            Dim ConfigFilename As String
            Dim Config As String
            Dim ConfigLines() As String
            '
            Dim Line As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            Dim NameValue() As String
            'Dim dataBuildVersion As String
            Dim SQLNow As String
            Dim SQL As String
            Dim DataSourceType As Integer
            'Dim KernelService As New KernelServicesClass
            Dim DatePtr As Integer
            Dim AddonInstall As coreAddonInstallClass
            Dim cp As CPClass
            Dim appList As List(Of String)
            'Dim control As New controlClass
            Dim apps() As String
            'Dim appRow As String = ""
            Dim appArg() As String
            Dim appName As String
            Dim appStatus As Integer
            '
            On Error Resume Next
            '
            ' put token in a config file
            '
            cp = New CPClass(appName)
            '
            rightNow = DateTime.Now()
            Yesterday = rightNow.AddDays(-1).Date
            ALittleWhileAgo = rightNow.AddDays(-90).Date
            SQLNow = cp.core.db.encodeSQLDate(rightNow)
            '
            ' ----- Read config file
            '
            ConfigFilename = "HouseKeepConfig.txt"
            Config = cp.core.privateFiles.readFile("config\" & ConfigFilename)
            If Config = "" Then
                Config = cp.core.privateFiles.readFile("" & ConfigFilename)
            End If
            If Config <> "" Then
                ConfigLines = Split(Config, vbCrLf)
                LineCnt = UBound(ConfigLines) + 1
                For LinePtr = 0 To LineCnt - 1
                    Line = Trim(ConfigLines(LinePtr))
                    If Line <> "" Then
                        NameValue = Split(Line, "=")
                        If UBound(NameValue) > 0 Then
                            If Trim(LCase(NameValue(0))) = "lastcheck" Then
                                If IsDate(NameValue(1)) Then
                                    LastCheckDateTime = CDate(NameValue(1))
                                End If
                                'Exit For
                            End If
                            If Trim(LCase(NameValue(0))) = "serverhousekeeptime" Then
                                If IsDate(NameValue(1)) Then
                                    ServerHousekeepTime = rightNow.Date.Add(EncodeDate(NameValue(1)).TimeOfDay)
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            Content = "" _
                & "lastcheck=" & rightNow & vbCrLf _
                & "serverhousekeeptime=" & ServerHousekeepTime & vbCrLf
            Call cp.core.privateFiles.saveFile("config\" & ConfigFilename, Content)
            '
            ' ----- Run Server Housekeep
            '
            If (rightNow.Date > LastCheckDateTime.Date) Then
                '
                ' new day since lastcheck, is alarm less then now
                '
                RunServerHousekeep = (ServerHousekeepTime < rightNow)
            Else
                '
                ' same day as lastcheck, is alarm between now and last time check
                '
                RunServerHousekeep = (rightNow > ServerHousekeepTime) And (LastCheckDateTime < ServerHousekeepTime)
            End If
            NewHour = rightNow.Hour <> LastCheckDateTime.Hour
            If DebugMode Or RunServerHousekeep Then
                '
                ' it is the next day, remove old log files
                '
                FolderName = "Logs"
                Call HousekeepLogFolder("server", FolderName)
                '
                Dim subDir As New System.IO.DirectoryInfo(cp.core.privateFiles.rootLocalPath & "\logs\")
                For Each SubDirInfo As System.IO.DirectoryInfo In subDir.GetDirectories
                    FolderName = "logs\" & SubDirInfo.Name
                Call HousekeepLogFolder("server", FolderName)
                Next
                'FolderName = "Logs\Email"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\Performance"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\HouseKeep"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\AddonInstall"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\Monitor"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\Admin"
                'Call HousekeepLogFolder("server", FolderName)
                ''
                'FolderName = "Logs\Process"
                'Call HousekeepLogFolder("server", FolderName)
                '
                ' Download Updates
                '
                Call DownloadUpdates()
                '
                ' Set LogCheckDate
                '
                LogCheckDateLast = Int(Now)
            End If
            '
            ' Housekeep each application
            '
            For Each kvp As KeyValuePair(Of String, Models.Entity.serverConfigModel.appConfigModel) In cp.core.serverConfig.apps
                appName = kvp.Value.name
                If True Then
                    'End If
                    ''20151109 - removed but unsure what in initContext can/must be moved to cp constructor
                    'If Not cp.execute_initContext() Then
                    '    '
                    '    ' failed to initialize, skip it
                    '    '
                    'Else
                    'appArg = Split(appRow, vbTab)
                    'appName = appArg(0)
                    appStatus = EncodeInteger(appName)
                    If appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady Then


                        '

                        cp = New CPClass(appName)
                        If Not (cp Is Nothing) Then
                            '20151109 - removed but unsure what in initContext can/must be moved to cp constructor
                            'Call cp.execute_initContext()
                            cpCore = cp.core
                            If True Then
                                '
                                ' Register and unregister files in the Addon folder
                                '
                                Call housekeepAddonFolder()
                                '
                                ' Upgrade Local Collections, and all applications that use them
                                '
                                ErrorMessage = ""
                                Call AppendClassLog("", "HouseKeep", "Updating local collections from library, see Upgrade log for details during this period.")
                                Dim ignoreRefactor As String = ""
                                AddonInstall = New coreAddonInstallClass(cp.core)
                                If Not AddonInstall.UpgradeLocalCollectionRepoFromRemoteCollectionRepo(ErrorMessage, ignoreRefactor, ignoreRefactor, False) Then
                                    If ErrorMessage = "" Then
                                        ErrorMessage = "No detailed error message was returned from UpgradeAllLocalCollectionsFromLib2 although it returned 'not ok' status."
                                    End If
                                    Call AppendClassLog("", "HouseKeep", "Updating local collections from Library returned an error, " & ErrorMessage)
                                End If
                                '
                                ' Verify core installation
                                '
                                Call AddonInstall.installCollectionFromRemoteRepo(CoreCollectionGuid, ErrorMessage, "", False)
                                '
                                '
                                '
                                DomainNameList = cp.core.app_domainList
                                DomainNamePrimary = DomainNameList
                                Pos = vbInstr(1, DomainNamePrimary, ",")
                                If Pos > 1 Then
                                    DomainNamePrimary = Mid(DomainNamePrimary, 1, Pos - 1)
                                End If
                                'dataBuildVersion = cp.Core.app.getSiteProperty("BuildVersion", "0")
                                DataSourceType = cp.core.db.getDataSourceType("default")
                                '
                                DefaultMemberName = ""
                                PeopleCID = cp.core.metaData.getContentId("people")
                                SQL = "select defaultvalue from ccfields where name='name' and contentid=(" & PeopleCID & ")"
                                CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                If cp.core.db.cs_ok(CS) Then
                                    DefaultMemberName = cp.core.db.cs_getText(CS, "defaultvalue")
                                End If
                                Call cp.core.db.cs_Close(CS)
                                '
                                ' Get ArchiveAgeDays - use this as the oldest data they care about
                                '
                                VisitArchiveAgeDays = EncodeInteger(cp.core.siteProperties.getText("ArchiveRecordAgeDays", "365"))
                                If (VisitArchiveAgeDays < 2) Then
                                    VisitArchiveAgeDays = 2
                                    Call cp.core.siteProperties.setProperty("ArchiveRecordAgeDays", "2")
                                End If
                                VisitArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date
                                OldestVisitSummaryWeCareAbout = Int(Now) - 120
                                If OldestVisitSummaryWeCareAbout < VisitArchiveDate Then
                                    OldestVisitSummaryWeCareAbout = VisitArchiveDate
                                End If
                                'OldestVisitSummaryWeCareAbout = Int(Now) - VisitArchiveAgeDays
                                '
                                ' Get GuestArchiveAgeDays
                                '
                                GuestArchiveAgeDays = EncodeInteger(cp.core.siteProperties.getText("ArchivePeopleAgeDays", "2"))
                                If (GuestArchiveAgeDays < 2) Then
                                    GuestArchiveAgeDays = 2
                                    Call cp.core.siteProperties.setProperty("ArchivePeopleAgeDays", CStr(GuestArchiveAgeDays))
                                End If
                                '
                                ' Get EmailDropArchiveAgeDays
                                '
                                EmailDropArchiveAgeDays = EncodeInteger(cp.core.siteProperties.getText("ArchiveEmailDropAgeDays", "90"))
                                If (EmailDropArchiveAgeDays < 2) Then
                                    EmailDropArchiveAgeDays = 2
                                    Call cp.core.siteProperties.setProperty("ArchiveEmailDropAgeDays", CStr(EmailDropArchiveAgeDays))
                                End If
                                '
                                ' Do non-optional housekeeping
                                '
                                If RunServerHousekeep Or DebugMode Then
                                    If True Then ' 3.3.971" Then
                                        '
                                        ' Move Archived pages from their current parent to their archive parent
                                        '
                                        Call AppendClassLog(appName, "HouseKeep", "Archive update for pages on [" & cp.core.serverConfig.appConfig.name & "]")
                                        SQL = "select * from ccpagecontent where (( DateArchive is not null )and(DateArchive<" & SQLNow & "))and(active<>0)"
                                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                        Do While cp.core.db.cs_ok(CS)
                                            ArchiveParentID = cp.core.db.cs_getInteger(CS, "ArchiveParentID")
                                            If ArchiveParentID = 0 Then
                                                SQL = "update ccpagecontent set DateArchive=null where (id=" & RecordID & ")or((editsourceid=" & RecordID & ")and(editarchive=0))"
                                                Call cp.core.db.executeSql(SQL)
                                            Else
                                                RecordID = cp.core.db.cs_getInteger(CS, "ID")
                                                SQL = "update ccpagecontent set ArchiveParentID=null,DateArchive=null,parentid=" & ArchiveParentID & " where (id=" & RecordID & ")or((editsourceid=" & RecordID & ")and(editarchive=0))"
                                                Call cp.core.db.executeSql(SQL)
                                                NeedToClearCache = True
                                            End If
                                            cp.core.db.cs_goNext(CS)
                                        Loop
                                        Call cp.core.db.cs_Close(CS)
                                        '
                                        ' Clear caches
                                        '
                                        If NeedToClearCache Then
                                            emptyData = Nothing
                                            Call cp.core.cache.invalidateTag("Page Content")
                                            Call cp.core.cache.setKey("PCC", emptyData)
                                        End If
                                    End If
                                    If True Then
                                        '
                                        ' Delete any daily visit summary duplicates during this period(keep the first)
                                        '
                                        SQL = "delete from ccvisitsummary" _
                                            & " where id in (" _
                                            & " select d.id from ccvisitsummary d,ccvisitsummary f" _
                                            & " where f.datenumber=d.datenumber" _
                                            & " and f.datenumber>" & cp.core.db.encodeSQLDate(OldestVisitSummaryWeCareAbout) _
                                            & " and f.datenumber<" & cp.core.db.encodeSQLDate(Yesterday) _
                                            & " and f.TimeDuration=24" _
                                            & " and d.TimeDuration=24" _
                                            & " and f.id<d.id" _
                                            & ")"
                                        Call cp.core.db.executeSql(SQL)
                                        '
                                        ' Find missing daily summaries, summarize that date
                                        '
                                        SQL = cp.core.db.GetSQLSelect("default", "ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" & OldestVisitSummaryWeCareAbout.Date.ToOADate, "DateNumber,TimeNumber")
                                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                        'If Not cp.Core.app.csv_IsCSOK(CS) Then
                                        '    '
                                        '    ' No data was found for this period, summarize the entire period
                                        '    '
                                        '    DatePtr = OldestVisitSummaryWeCareAbout
                                        'Else
                                        For DatePtr = OldestVisitSummaryWeCareAbout.ToOADate To Yesterday.ToOADate
                                            If Not cp.core.db.cs_ok(CS) Then
                                                '
                                                ' Out of data, start with this DatePtr
                                                '
                                                Call HouseKeep_VisitSummary(Date.FromOADate(DatePtr), Date.FromOADate(DatePtr), 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout)
                                                'Exit For
                                            Else
                                                DateNumberWorking = cp.core.db.cs_getInteger(CS, "DateNumber")
                                                If DatePtr < DateNumberWorking Then
                                                    '
                                                    ' There are missing dates, update them
                                                    '
                                                    Call HouseKeep_VisitSummary(Date.FromOADate(DatePtr), Date.FromOADate(DateNumberWorking - 1), 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout)
                                                End If
                                            End If
                                            If cp.core.db.cs_ok(CS) Then
                                                '
                                                ' if there is more data, go to the next record
                                                '
                                                Call cp.core.db.cs_goNext(CS)
                                            End If
                                        Next
                                        'End If
                                        Call cp.core.db.cs_Close(CS)
                                        'Call HouseKeep_VisitSummary( CDate(DatePtr), RightNow, 24, BuildVersion, OldestVisitSummaryWeCareAbout)

                                        '                    SQL = cp.Core.app.csv_GetSQLSelect("default", "ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" & Int(CDbl(OldestVisitSummaryWeCareAbout)), "DateNumber Desc", , 1)
                                        '                    CS = cp.Core.app.csv_OpenCSSQL("default", SQL)
                                        '                    If Not cp.Core.app.csv_IsCSOK(CS) Then
                                        '                        OldestSummarizedDateNumber = OldestVisitSummaryWeCareAbout
                                        '                    Else
                                        '                        OldestSummarizedDateNumber = cp.Core.app.csv_cs_getNumber(CS, "DateNumber")
                                        '                        If OldestSummarizedDateNumber > OldestVisitSummaryWeCareAbout Then
                                        '                            OldestSummarizedDateNumber = OldestVisitSummaryWeCareAbout
                                        '                        End If
                                        '                    End If
                                        '                    Call cp.Core.app.csv_CloseCS(CS)
                                        '                    Call HouseKeep_VisitSummary( CDate(OldestSummarizedDateNumber), RightNow, 24, BuildVersion, OldestVisitSummaryWeCareAbout)

                                    End If
                                    If True Then
                                        '
                                        ' Remote Query Expiration
                                        '
                                        SQL = "delete from ccRemoteQueries where (DateExpires is not null)and(DateExpires<" & cp.core.db.encodeSQLDate(Now()) & ")"
                                        Call cp.core.db.executeSql(SQL)
                                    End If
                                    If True Then
                                        '
                                        ' Clean Navigation
                                        '
                                        If DataSourceType = DataSourceTypeODBCMySQL Then
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null"
                                        Else
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null)"
                                        End If
                                        Call cp.core.db.executeSql(SQL)
                                        '
                                        If DataSourceType = DataSourceTypeODBCMySQL Then
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null"
                                        Else
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null)"
                                        End If
                                        Call cp.core.db.executeSql(SQL)
                                        '
                                        If DataSourceType = DataSourceTypeODBCMySQL Then
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpcollectionid where m.helpcollectionid<>0 and a.id is null"
                                        Else
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAddonCollections c on c.id=m.helpcollectionid Where m.helpcollectionid <> 0 And c.Id Is Null)"
                                        End If
                                        Call cp.core.db.executeSql(SQL)
                                    End If
                                    '
                                    ' Page View Summary
                                    '
                                    If True Then ' 4.1.187" Then
                                        ''
                                        '' Delete duplicates
                                        ''
                                        'SQL = "delete from ccviewingsummary" _
                                        '    & " where id in (" _
                                        '    & " select d.id from ccviewingsummary d,ccviewingsummary f" _
                                        '    & " where f.datenumber=d.datenumber" _
                                        '    & " and f.datenumber>" & encodeSQLDate(OldestVisitSummaryWeCareAbout) _
                                        '    & " and f.datenumber<" & encodeSQLDate(Yesterday) _
                                        '    & " and f.TimeDuration=24" _
                                        '    & " and d.TimeDuration=24" _
                                        '    & " and f.id<d.id" _
                                        '    & ")"
                                        'Call cp.Core.app.ExecuteSQL( SQL)
                                        '
                                        ' Find the day of the last entry in the viewing summary table as start there
                                        ' PageViewSummary should always add at least one entry for each day, even if 0
                                        '
                                        SQL = cp.core.db.GetSQLSelect("default", "ccviewingsummary", "DateNumber", "TimeDuration=24 and DateNumber>=" & OldestVisitSummaryWeCareAbout.Date.ToOADate, "DateNumber Desc", , 1)
                                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                        If Not cp.core.db.cs_ok(CS) Then
                                            DatePtr = CLng(Int(OldestVisitSummaryWeCareAbout))
                                        Else
                                            DatePtr = cp.core.db.cs_getInteger(CS, "DateNumber")
                                        End If
                                        Call cp.core.db.cs_Close(CS)
                                        If DatePtr < CLng(Int(OldestVisitSummaryWeCareAbout)) Then
                                            DatePtr = CLng(Int(OldestVisitSummaryWeCareAbout))
                                        End If
                                        Call HouseKeep_PageViewSummary(Date.FromOADate(DatePtr), Yesterday, 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout)
                                        '                    For DatePtr = OldestVisitSummaryWeCareAbout To Int(Yesterday)
                                        '                        If Not cp.Core.app.csv_IsCSOK(CS) Then
                                        '                            '
                                        '                            ' Out of data, start with this DatePtr
                                        '                            '
                                        '                            Call HouseKeep_PageViewSummary( CDate(DatePtr), CDate(DatePtr), 24, BuildVersion, OldestVisitSummaryWeCareAbout)
                                        '                            'Exit For
                                        '                        Else
                                        '                            DateNumberWorking = cp.Core.app.csv_cs_getInteger(CS, "DateNumber")
                                        '                            If DatePtr < DateNumberWorking Then
                                        '                                '
                                        '                                ' There are missing dates, update them
                                        '                                '
                                        '                                Call HouseKeep_PageViewSummary( CDate(DatePtr), CDate(DateNumberWorking - 1), 24, BuildVersion, OldestVisitSummaryWeCareAbout)
                                        '                            End If
                                        '                        End If
                                        '                        If cp.Core.app.csv_IsCSOK(CS) Then
                                        '                            '
                                        '                            ' if there is more data, go to the next record
                                        '                            '
                                        '                            Call cp.Core.app.csv_NextCSRecord(CS)
                                        '                        End If
                                        '                    Next
                                        '                    Call cp.Core.app.csv_CloseCS(CS)
                                    End If
                                End If
                                '
                                ' Each hour, summarize the visits and viewings into the Visit Summary table
                                '
                                If (DebugMode Or NewHour) Then
                                    '
                                    ' Set NextSummaryStartDate based on the last time we ran hourly summarization
                                    '
                                    LastTimeSummaryWasRun = VisitArchiveDate
                                    'LastTimeSummaryWasRun = ALittleWhileAgo
                                    'sql="select top 1 dateadded from ccvisitsummary where (timeduration=1)and(Dateadded>" & encodeSQLDate(ALittleWhileAgo) & ") order by id desc"
                                    SQL = cp.core.db.GetSQLSelect("default", "ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" & cp.core.db.encodeSQLDate(VisitArchiveDate) & ")", "id Desc", , 1)
                                    'SQL = cp.Core.app.csv_GetSQLSelect("default", "ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" & encodeSQLDate(ALittleWhileAgo) & ")", "id Desc", , 1)
                                    CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                    If cp.core.db.cs_ok(CS) Then
                                        LastTimeSummaryWasRun = cp.core.db.cs_getDate(CS, "DateAdded")
                                        Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, last time summary was run was [" & LastTimeSummaryWasRun & "]")
                                    Else
                                        Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, no hourly summaries were found, set start to [" & LastTimeSummaryWasRun & "]")
                                    End If
                                    Call cp.core.db.cs_Close(CS)
                                    NextSummaryStartDate = LastTimeSummaryWasRun
                                    '
                                    ' Each hourly entry includes visits that started during that hour, but we do not know when they finished (maybe during last hour)
                                    '   Find the oldest starttime of all the visits with endtimes after the LastTimeSummaryWasRun. Resummarize all periods
                                    '   from then to now
                                    '
                                    '   For the past 24 hours, find the oldest visit with the last viewing during the last hour
                                    '
                                    'OldestDateAdded = LastTimeSummaryWasRun
                                    'PeriodStep = CDbl(1) / CDbl(24)
                                    StartOfHour = New Date(LastTimeSummaryWasRun.Year, LastTimeSummaryWasRun.Month, LastTimeSummaryWasRun.Day, LastTimeSummaryWasRun.Hour, 1, 1).AddHours(-1) ' (Int(24 * LastTimeSummaryWasRun) / 24) - PeriodStep
                                    OldestDateAdded = StartOfHour
                                    SQL = cp.core.db.GetSQLSelect("default", "ccVisits", "DateAdded", "LastVisitTime>" & cp.core.db.encodeSQLDate(StartOfHour), "dateadded", , 1)
                                    'SQL = "select top 1 Dateadded from ccvisits where LastVisitTime>" & encodeSQLDate(StartOfHour) & " order by DateAdded"
                                    CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                    If cp.core.db.cs_ok(CS) Then
                                        OldestDateAdded = cp.core.db.cs_getDate(CS, "DateAdded")
                                        If OldestDateAdded < NextSummaryStartDate Then
                                            NextSummaryStartDate = OldestDateAdded
                                            Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, found a visit with the last viewing during the past hour. It started [" & OldestDateAdded & "], before the last summary was run.")
                                        End If
                                    End If
                                    Call cp.core.db.cs_Close(CS)
                                    '
                                    ' Verify there are 24 hour records for every day back the past 90 days
                                    '
                                    Dim DateofMissingSummary As Date
                                    DateofMissingSummary = Date.MinValue
                                    'Call AppendClassLog(cp.Core.appEnvironment.name, "HouseKeep", "Verify there are 24 hour records for the past 90 days")
                                    PeriodStart = Int(rightNow) - 90
                                    PeriodStep = 1
                                    For PeriodDatePtr = PeriodStart.ToOADate To OldestDateAdded.ToOADate Step PeriodStep
                                        SQL = "select count(id) as HoursPerDay from ccVisitSummary where TimeDuration=1 and DateNumber=" & CLng(PeriodDatePtr) & " group by DateNumber"
                                        'SQL = "select count(id) as HoursPerDay from ccVisitSummary group by DateNumber having DateNumber=" & CLng(PeriodDatePtr)
                                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                        HoursPerDay = 0
                                        If cp.core.db.cs_ok(CS) Then
                                            HoursPerDay = cp.core.db.cs_getInteger(CS, "HoursPerDay")
                                        End If
                                        Call cp.core.db.cs_Close(CS)
                                        If HoursPerDay < 24 Then
                                            DateofMissingSummary = Date.FromOADate(PeriodDatePtr)
                                            Exit For
                                        End If
                                    Next
                                    If (DateofMissingSummary <> Date.MinValue) And (DateofMissingSummary < NextSummaryStartDate) Then
                                        Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep", "Found a missing hourly period in the visit summary table [" & DateofMissingSummary & "], it only has [" & HoursPerDay & "] hourly summaries.")
                                        NextSummaryStartDate = DateofMissingSummary
                                    End If
                                    '
                                    ' Now summarize all visits during all hourly periods between OldestDateAdded and the previous Hour
                                    '
                                    Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep", "Summaryize visits hourly, starting [" & NextSummaryStartDate & "]")
                                    PeriodStep = CDbl(1) / CDbl(24)
                                    'PeriodStart = (Int(OldestDateAdded * 24) / 24)
                                    Call HouseKeep_VisitSummary(NextSummaryStartDate, rightNow, 1, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout)
                                End If
                                '
                                ' OK to run archive
                                ' During archive, non-cookie records are removed, so this has to run after summarizing
                                ' and we can only delete non-cookie records older than 2 days (so we can be sure they have been summarized)
                                '
                                If DebugMode Then
                                    '
                                    ' debug mode - run achive if no times are given
                                    '
                                    Call HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion)
                                Else
                                    '
                                    ' Check for site's archive time of day
                                    '
                                    AlarmTimeString = cp.core.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM")
                                    If AlarmTimeString = "" Then
                                        AlarmTimeString = "12:00:00 AM"
                                        Call cp.core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString)
                                    End If
                                    If Not IsDate(AlarmTimeString) Then
                                        AlarmTimeString = "12:00:00 AM"
                                        Call cp.core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString)
                                    End If
                                    AlarmTimeMinutesSinceMidnight = EncodeDate(AlarmTimeString).TimeOfDay.TotalMinutes
                                    minutesSinceMidnight = rightNow.TimeOfDay.TotalMinutes
                                    LastCheckMinutesFromMidnight = LastCheckDateTime.TimeOfDay.TotalMinutes
                                    If (minutesSinceMidnight > LastCheckMinutesFromMidnight) And (LastCheckMinutesFromMidnight >= LastCheckMinutesFromMidnight) And (LastCheckMinutesFromMidnight < minutesSinceMidnight) Then
                                        '
                                        ' Same Day - Midnight is before last and after current
                                        '
                                        Call HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion)
                                    ElseIf (LastCheckMinutesFromMidnight > minutesSinceMidnight) And ((LastCheckMinutesFromMidnight < minutesSinceMidnight) Or (LastCheckMinutesFromMidnight > LastCheckMinutesFromMidnight)) Then
                                        '
                                        ' New Day - Midnight is between Last and Set
                                        '
                                        Call HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion)
                                    End If
                                End If
                            End If
                            cpCore = Nothing
                        End If
                        Call cp.Dispose()
                        cp = Nothing
                    End If
                End If
            Next
            '
            Exit Sub
ErrorTrap:
            Call HandleClassTrapError("", "HouseKeep", "Trap", True)
            Err.Clear()
        End Sub
        '
        '
        '
        Private Sub HouseKeep_App_Daily(ByVal VisitArchiveAgeDays As Integer, ByVal GuestArchiveAgeDays As Integer, ByVal EmailDropArchiveAgeDays As Integer, ByVal DefaultMemberName As String, ByVal BuildVersion As String)
            On Error GoTo ErrorTrap
            '
            Dim ArchiveEmailDropDate As Date
            Dim VirtualFileName As String
            Dim VirtualLink As String
            Dim FilenameOriginal As String
            Dim Pos As Integer
            Dim FilenameExt As String
            Dim FilenameNoExt As String
            Dim FilenameAltSize As String
            Dim FileList As IO.FileInfo()
            '
            Dim FileSize As Integer
            Dim PathNameRev As String
            Dim FilenameDim() As String
            '
            Dim DaystoRemove As Integer
            Dim fieldType As Integer
            Dim FieldContentID As Integer
            Dim FieldCaption As String
            Dim FieldLast As String
            Dim FieldNew As String
            Dim FieldRecordID As Integer
            Dim RecordID As Integer
            Dim ArchiveParentID As Integer
            Dim OldestVisitDate As Date
            Dim ArchiveDate As Date
            Dim thirtyDaysAgo As Date
            Dim SingleDate As Date
            Dim DataSourceType As Integer
            '
            'Dim Controller As controlClass
            Dim VisitArchiveDeleteSize As Integer
            ''Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            '    Dim CSConnection As appEnvironmentStruc
            Dim SQL As String
            Dim SQLCriteria As String
            Dim PathName As String
            Dim TableName As String
            Dim FieldName As String
            Dim CS As Integer
            Dim CSTest As Integer
            Dim Filename As String
            Dim FileSplit() As String
            Dim FolderName As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            Dim AdminLicenseCount As Integer
            Dim ArchiveDateSQL As String
            Dim appName As String
            Dim SQLTablePeople As String
            Dim SQLTableMemberRules As String
            Dim SQLTableGroups As String
            Dim PeopleCID As Integer
            'Dim DefaultName As String
            Dim Hint As String
            Dim ArchiveDeleteNoCookie As Boolean
            Dim MidnightTwoDaysAgo As Date
            Dim SQLDateMidnightTwoDaysAgo As String
            Dim TimeoutSave As Integer
            Dim Yesterday As Date
            Dim rightNow As Date = DateTime.Now
            '
            Yesterday = rightNow.AddDays(-1).Date
            MidnightTwoDaysAgo = rightNow.AddDays(-2).Date
            thirtyDaysAgo = rightNow.AddDays(-30).Date
            appName = cp.core.serverConfig.appConfig.name
            ArchiveDeleteNoCookie = EncodeBoolean(cp.core.siteProperties.getText("ArchiveDeleteNoCookie", "1"))
            DataSourceType = cp.core.db.getDataSourceType("default")
            TimeoutSave = cp.core.db.sqlCommandTimeout
            cp.core.db.sqlCommandTimeout = 1800
            '
            SQLTablePeople = cp.core.metaData.getContentTablename("People")
            SQLTableMemberRules = cp.core.metaData.getContentTablename("Member Rules")
            SQLTableGroups = cp.core.metaData.getContentTablename("Groups")
            SQLDateMidnightTwoDaysAgo = cp.core.db.encodeSQLDate(MidnightTwoDaysAgo)
            '
            ' Any member records that were created outside contensive need to have CreatedByVisit=0 (past v4.1.152)
            '
            If True Then
                '
                ' it was this upgrade when I updated all the old membes to CreatedByVisit=1
                '
                SQL = "update ccmembers set CreatedByVisit=0 where createdbyvisit is null"
                On Error Resume Next
                Call cp.core.db.executeSql(SQL)
                If Err.Number <> 0 Then
                    Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
                End If
                Err.Clear()
                On Error GoTo ErrorTrap
            End If
            '
            ' delete nocookie visits
            ' This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
            '
            If ArchiveDeleteNoCookie Then
                '
                ' delete members from the non-cookie visits
                ' legacy records without createdbyvisit will have to be corrected by hand (or upgrade)
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting members from visits with no cookie support older than Midnight, Two Days Ago")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete m.*" _
                            & " from " & SQLTablePeople & " m,ccvisits v" _
                            & " where v.memberid=m.id" _
                            & " and(m.Visits=1)" _
                            & " and(m.createdbyvisit=1)" _
                            & " and(m.Username is null)" _
                            & " and(m.email is null)" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case DataSourceTypeODBCMySQL
                        SQL = "delete m" _
                            & " from " & SQLTablePeople & " m,ccvisits v" _
                            & " where v.memberid=m.id" _
                            & " and(m.Visits=1)" _
                            & " and(m.createdbyvisit=1)" _
                            & " and(m.Username is null)" _
                            & " and(m.email is null)" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case Else
                        SQL = "delete from " & SQLTablePeople _
                            & " from " & SQLTablePeople & " m,ccvisits v" _
                            & " where v.memberid=m.id" _
                            & " and(m.Visits=1)" _
                            & " and(m.createdbyvisit=1)" _
                            & " and(m.Username is null)" _
                            & " and(m.email is null)" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                End Select
                ' removed name requirement bc spiders not have bot names
                '        Select Case DataSourceType
                '            Case DataSourceTypeODBCAccess
                '                SQL = "delete m.*" _
                '                    & " from " & SQLTablePeople & " m,ccvisits v" _
                '                    & " where v.memberid=m.id" _
                '                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                '                    & " and(m.Visits=1)" _
                '                    & " and(m.Username is null)" _
                '                    & " and(m.email is null)" _
                '                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                '            Case DataSourceTypeODBCMySQL
                '                SQL = "delete m" _
                '                    & " from " & SQLTablePeople & " m,ccvisits v" _
                '                    & " where v.memberid=m.id" _
                '                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                '                    & " and(m.Visits=1)" _
                '                    & " and(m.Username is null)" _
                '                    & " and(m.email is null)" _
                '                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                '            Case Else
                '                SQL = "delete from " & SQLTablePeople _
                '                    & " from " & SQLTablePeople & " m,ccvisits v" _
                '                    & " where v.memberid=m.id" _
                '                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                '                    & " and(m.Visits=1)" _
                '                    & " and(m.Username is null)" _
                '                    & " and(m.email is null)" _
                '                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                '        End Select
                ' if this fails, continue with the rest of the work
                On Error Resume Next
                Call cp.core.db.executeSql(SQL)
                If Err.Number <> 0 Then
                    Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
                End If
                Err.Clear()
                On Error GoTo ErrorTrap
                '
                ' delete viewings from the non-cookie visits
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting viewings from visits with no cookie support older than Midnight, Two Days Ago")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete h.*" _
                            & " from ccviewings h,ccvisits v" _
                            & " where h.visitid=v.id" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case DataSourceTypeODBCMySQL
                        SQL = "delete h" _
                            & " from ccviewings h,ccvisits v" _
                            & " where h.visitid=v.id" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case Else
                        SQL = "delete from ccviewings" _
                            & " from ccviewings h,ccvisits v" _
                            & " where h.visitid=v.id" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                End Select
                ' if this fails, continue with the rest of the work
                On Error Resume Next
                Call cp.core.db.executeSql(SQL)
                If Err.Number <> 0 Then
                    Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
                End If
                Err.Clear()
                On Error GoTo ErrorTrap
                '
                ' delete visitors from the non-cookie visits
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visitors from visits with no cookie support older than Midnight, Two Days Ago")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete r.*" _
                            & " from ccvisitors r,ccvisits v" _
                            & " where r.id=v.visitorid" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case DataSourceTypeODBCMySQL
                        SQL = "delete r" _
                            & " from ccvisitors r,ccvisits v" _
                            & " where r.id=v.visitorid" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    Case Else
                        SQL = "delete from ccvisitors" _
                            & " from ccvisitors r,ccvisits v" _
                            & " where r.id=v.visitorid" _
                            & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                End Select
                ' if this fails, continue with the rest of the work
                On Error Resume Next
                Call cp.core.db.executeSql(SQL)
                If Err.Number <> 0 Then
                    Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
                End If
                Err.Clear()
                On Error GoTo ErrorTrap
                '
                ' delete visits from the non-cookie visits
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visits with no cookie support older than Midnight, Two Days Ago")
                Call cp.core.DeleteTableRecordChunks("default", "ccvisits", "(CookieSupport=0)and(LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")", 1000, 10000)
            End If
            '
            ' Visits with no DateAdded
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visits with no DateAdded")
            Call cp.core.DeleteTableRecordChunks("default", "ccvisits", "(DateAdded is null)or(DateAdded<=" & cp.core.db.encodeSQLDate("1/1/1995") & ")", 1000, 10000)
            '
            ' Visits with no visitor
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visits with no DateAdded")
            Call cp.core.DeleteTableRecordChunks("default", "ccvisits", "(VisitorID is null)or(VisitorID=0)", 1000, 10000)
            '
            ' viewings with no visit
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting viewings with null or invalid VisitID")
            Call cp.core.DeleteTableRecordChunks("default", "ccviewings", "(visitid=0 or visitid is null)", 1000, 10000)
            '
            ' Get Oldest Visit
            '
            'SQL = "select top 1 DateAdded from ccVisits where dateadded>0 order by DateAdded"
            SQL = cp.core.db.GetSQLSelect("default", "ccVisits", "DateAdded", , "dateadded", , 1)
            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
            If cp.core.db.cs_ok(CS) Then
                OldestVisitDate = Int(cp.core.db.cs_getDate(CS, "DateAdded"))
            End If
            Call cp.core.db.cs_Close(CS)
            '
            ' Remove old visit records
            '   if > 30 days in visit table, limit one pass to just 30 days
            '   this is to prevent the entire server from being bogged down for one site change
            '
            If OldestVisitDate = Date.MinValue Then
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "No records were removed because no visit records were found while requesting the oldest visit.")
            ElseIf (VisitArchiveAgeDays <= 0) Then
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "No records were removed because Housekeep ArchiveRecordAgeDays is 0.")
            Else
                ArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date
                DaystoRemove = (ArchiveDate - OldestVisitDate).TotalDays
                If DaystoRemove > 30 Then
                    ArchiveDate = OldestVisitDate.AddDays(30)
                End If
                If OldestVisitDate >= ArchiveDate Then
                    Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "No records were removed because Oldest Visit Date [" & OldestVisitDate & "] >= ArchiveDate [" & ArchiveDate & "].")
                Else
                    Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Removing records from [" & OldestVisitDate & "] to [" & ArchiveDate & "].")
                    SingleDate = OldestVisitDate
                    Do
                        Call HouseKeep_App_Daily_RemoveVisitRecords(SingleDate, DataSourceType)
                        SingleDate = SingleDate.AddDays(1)
                    Loop While SingleDate < ArchiveDate
                End If
            End If
            '
            ' Remove old guest records
            '
            ArchiveDate = rightNow.AddDays(-GuestArchiveAgeDays).Date
            Call HouseKeep_App_Daily_RemoveGuestRecords(ArchiveDate, DataSourceType)
            '
            ' delete 'guests' Members with one visits but no valid visit record
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete m.*" _
                        & " from " & SQLTablePeople & " m,ccvisits v" _
                        & " where v.memberid=m.id" _
                        & " and(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
                Case DataSourceTypeODBCMySQL
                    SQL = "delete m" _
                        & " from " & SQLTablePeople & " m,ccvisits v" _
                        & " where v.memberid=m.id" _
                        & " and(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
                Case Else
                    SQL = "delete from " & SQLTablePeople _
                        & " from " & SQLTablePeople & " m,ccvisits v" _
                        & " where v.memberid=m.id" _
                        & " and(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
            End Select
            On Error Resume Next
            Call cp.core.db.executeSql(SQL)
            If Err.Number <> 0 Then
                Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            ' moved to upgrade code
            '    '
            '    ' Update CreatedByVisit for older records where this field is null
            '    '
            '    On Error Resume Next
            '    SQL = "update ccmembers set createdbyvisit=1 where (createdbyvisit Is Null) And (dateadded<" & encodeSQLDate("1/1/2010") & ") and (username Is Null) And (email Is Null) And ((visits <> 0) And (visits Is Not Null))"
            '    Call cp.Core.app.ExecuteSQL( SQL)
            '    Err.Clear
            '    On Error GoTo ErrorTrap
            '
            ' delete 'guests' Members created before ArchivePeopleAgeDays
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete m.*" _
                        & " from " & SQLTablePeople & " m left join ccvisits v on v.memberid=m.id" _
                        & " where(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
                Case DataSourceTypeODBCMySQL
                    SQL = "delete m" _
                        & " from " & SQLTablePeople & " m left join ccvisits v on v.memberid=m.id" _
                        & " where(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
                Case Else
                    SQL = "delete from " & SQLTablePeople _
                        & " from " & SQLTablePeople & " m left join ccvisits v on v.memberid=m.id" _
                        & " where(m.createdbyvisit=1)" _
                        & " and(m.Visits=1)" _
                        & " and(m.Username is null)" _
                        & " and(m.email is null)" _
                        & " and(m.dateadded=m.lastvisit)" _
                        & " and(v.id is null)"
            End Select
            On Error Resume Next
            Call cp.core.db.executeSql(SQL)
            If Err.Number <> 0 Then
                Call HandleClassTrapError(appName, "HouseKeep_App_Daily", GetErrString(Err), True)
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            '
            ' delete email drops older than archive.
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting email drops older then " & EmailDropArchiveAgeDays & " days")
            ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date
            On Error Resume Next
            Call cp.core.db.deleteContentRecords("Email drops", "(DateAdded is null)or(DateAdded<=" & cp.core.db.encodeSQLDate(ArchiveEmailDropDate) & ")")
            If Err.Number <> 0 Then
                Call HandleClassTrapError(appName, "HouseKeep_App_Daily", "Error while deleting old email drops, " & GetErrString(Err), True)
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            '
            ' delete email log entries not realted to a drop, older than archive.
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting non-drop email logs older then " & EmailDropArchiveAgeDays & " days")
            ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date
            On Error Resume Next
            Call cp.core.db.deleteContentRecords("Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" & cp.core.db.encodeSQLDate(ArchiveEmailDropDate) & "))")
            If Err.Number <> 0 Then
                Call HandleClassTrapError(appName, "HouseKeep_App_Daily", "Error while deleting old email log, " & GetErrString(Err), True)
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            '
            ' delete email log entries without email drops
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting drop email log entries for drops without a valid drop record.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete l.*" _
                        & " from ccemaillog l" _
                        & " left join ccemaildrops d on d.id=l.emaildropid" _
                        & " where l.emaildropid Is Not Null" _
                        & " and d.id is null" _
                        & ""
                Case DataSourceTypeODBCMySQL
                    SQL = "delete l" _
                        & " from ccemaillog l" _
                        & " left join ccemaildrops d on d.id=l.emaildropid" _
                        & " where l.emaildropid Is Not Null" _
                        & " and d.id is null" _
                        & ""
                Case Else
                    SQL = "delete from ccemaillog" _
                        & " from ccemaillog l" _
                        & " left join ccemaildrops d on d.id=l.emaildropid" _
                        & " where l.emaildropid Is Not Null" _
                        & " and d.id is null" _
                        & ""
            End Select
            On Error Resume Next
            Call cp.core.db.executeSql(SQL)
            If Err.Number <> 0 Then
                Call HandleClassTrapError(appName, "HouseKeep_App_Daily", "Deleting email log entries for drops without a valid drop record, " & GetErrString(Err), True)
            End If
            Err.Clear()
            On Error GoTo ErrorTrap

            '
            ' block duplicate redirect fields (match contentid+fieldtype+caption)
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Inactivate duplicate redirect fields")
            CS = cp.core.db.cs_openCsSql_rev("Default", "Select ID, ContentID, Type, Caption from ccFields where (active<>0)and(Type=" & FieldTypeIdRedirect & ") Order By ContentID, Caption, ID")
            FieldLast = ""
            Do While cp.core.db.cs_ok(CS)
                'FieldType = cp.Core.app.csv_cs_getInteger(CS, "Type")
                FieldContentID = cp.core.db.cs_getInteger(CS, "Contentid")
                FieldCaption = cp.core.db.cs_getText(CS, "Caption")
                FieldNew = FieldContentID & FieldCaption
                If (FieldNew = FieldLast) Then
                    FieldRecordID = cp.core.db.cs_getInteger(CS, "ID")
                    Call cp.core.db.executeSql("Update ccFields set active=0 where ID=" & FieldRecordID & ";")
                End If
                FieldLast = FieldNew
                Call cp.core.db.cs_goNext(CS)
            Loop
            Call cp.core.db.cs_Close(CS)
            '
            ' block duplicate non-redirect fields (match contentid+fieldtype+name)
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Inactivate duplicate non-redirect fields")
            CS = cp.core.db.cs_openCsSql_rev("Default", "Select ID, Name, ContentID, Type from ccFields where (active<>0)and(Type<>" & FieldTypeIdRedirect & ") Order By ContentID, Name, Type, ID")
            FieldLast = ""
            Do While cp.core.db.cs_ok(CS)
                fieldType = cp.core.db.cs_getInteger(CS, "Type")
                FieldContentID = cp.core.db.cs_getInteger(CS, "Contentid")
                FieldName = cp.core.db.cs_getText(CS, "Name")
                FieldRecordID = cp.core.db.cs_getInteger(CS, "ID")
                FieldNew = FieldContentID & FieldName & fieldType
                If (FieldNew = FieldLast) Then
                    Call cp.core.db.executeSql("Update ccFields set active=0 where ID=" & FieldRecordID & ";")
                End If
                FieldLast = FieldNew
                Call cp.core.db.cs_goNext(CS)
            Loop
            Call cp.core.db.cs_Close(CS)
            '
            ' Activities with no Member
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting activities with no member record.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccactivitylog.*" _
                        & " From ccactivitylog LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccactivitylog.memberid" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete from ccactivitylog" _
                        & " From ccactivitylog LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccactivitylog.memberid" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccactivitylog" _
                        & " From ccactivitylog LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccactivitylog.memberid" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' Member Properties with no member
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting member properties with no member record.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccProperties.*" _
                        & " From ccProperties LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=0)" _
                        & " AND (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete From ccProperties" _
                        & " From ccProperties LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=0)" _
                        & " AND (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccProperties" _
                        & " From ccProperties LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=0)" _
                        & " AND (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' Visit Properties with no visits
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visit properties with no visit record.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccProperties.*" _
                        & " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=1)" _
                        & " AND (ccVisits.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete From ccProperties" _
                        & " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=1)" _
                        & " AND (ccVisits.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccProperties" _
                        & " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID" _
                        & " WHERE (ccProperties.TypeID=1)" _
                        & " AND (ccVisits.ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' Visitor Properties with no visitor
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting visitor properties with no visitor record.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccProperties.*" _
                        & " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID" _
                        & " where ccproperties.typeid=2" _
                        & " and ccvisitors.id is null"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete From ccProperties" _
                        & " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID" _
                        & " where ccproperties.typeid=2" _
                        & " and ccvisitors.id is null"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccProperties" _
                        & " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID" _
                        & " where ccproperties.typeid=2" _
                        & " and ccvisitors.id is null"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' MemberRules with bad MemberID
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Member Rules with bad MemberID.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete " & SQLTableMemberRules & ".*" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=" & SQLTableMemberRules & ".MemberID" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete From " & SQLTableMemberRules & "" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=" & SQLTableMemberRules & ".MemberID" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete " & SQLTableMemberRules & "" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTablePeople & " on " & SQLTablePeople & ".ID=" & SQLTableMemberRules & ".MemberID" _
                        & " WHERE (" & SQLTablePeople & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' MemberRules with bad GroupID
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Member Rules with bad GroupID.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete " & SQLTableMemberRules & ".*" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=" & SQLTableMemberRules & ".GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete From " & SQLTableMemberRules & "" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=" & SQLTableMemberRules & ".GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete " & SQLTableMemberRules & "" _
                        & " From " & SQLTableMemberRules & "" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=" & SQLTableMemberRules & ".GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' GroupRules with bad ContentID
            '   Handled record by record removed to prevent CDEF reload
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Group Rules with bad ContentID.")
            SQL = "Select ccGroupRules.ID" _
                & " From ccGroupRules LEFT JOIN ccContent on ccContent.ID=ccGroupRules.ContentID" _
                & " WHERE (ccContent.ID is null)"
            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
            Do While cp.core.db.cs_ok(CS)
                Call cp.core.db.deleteContentRecord("Group Rules", cp.core.db.cs_getInteger(CS, "ID"))
                Call cp.core.db.cs_goNext(CS)
            Loop
            Call cp.core.db.cs_Close(CS)
            '
            ' GroupRules with bad GroupID
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Group Rules with bad GroupID.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccGroupRules.*" _
                        & " From ccGroupRules" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=ccGroupRules.GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete from ccGroupRules" _
                        & " From ccGroupRules" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=ccGroupRules.GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccGroupRules" _
                        & " From ccGroupRules" _
                        & " LEFT JOIN " & SQLTableGroups & " on " & SQLTableGroups & ".ID=ccGroupRules.GroupID" _
                        & " WHERE (" & SQLTableGroups & ".ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            ''
            '' TopicRules with bad ContentID
            '' delete manually to prevent cdef reload
            ''
            'Call AppendClassLog(AppName, "HouseKeep_App_Daily(" & AppName & ")", "Deleting Topic Rules with bad ContentID.")
            'SQL = "Select ccTopicRules.ID" _
            '    & " From ccTopicRules LEFT JOIN ccContent on ccContent.ID=ccTopicRules.ContentID" _
            '    & " WHERE (ccContent.ID is null)"
            'CS = cp.Core.app.csv_OpenCSSQL("default", SQL)
            'Do While cp.Core.app.csv_IsCSOK(CS)
            '    Call cp.Core.csv_DeleteContentRecord("Topic Rules", cp.Core.app.csv_cs_getInteger(CS, "ID"))
            '    Call cp.Core.app.csv_NextCSRecord(CS)
            '    Loop
            'Call cp.Core.app.csv_CloseCS(CS)
            ''
            '' TopicRules with bad TopicID
            ''
            'Call AppendClassLog(AppName, "HouseKeep_App_Daily(" & AppName & ")", "Deleting Topic Rules with bad TopicID.")
            'Select Case DataSourceType
            '    Case DataSourceTypeODBCAccess
            '        SQL = "delete ccTopicRules.*" _
            '            & " From ccTopicRules" _
            '            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
            '            & " WHERE (ccTopics.ID is null)"
            '        Call cp.Core.app.executeSql(sql)
            '    Case DataSourceTypeODBCSQLServer
            '        SQL = "delete from ccTopicRules" _
            '            & " From ccTopicRules" _
            '            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
            '            & " WHERE (ccTopics.ID is null)"
            '        Call cp.Core.app.executeSql(sql)
            '    Case Else
            '        SQL = "delete ccTopicRules" _
            '            & " From ccTopicRules" _
            '            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
            '            & " WHERE (ccTopics.ID is null)"
            '        Call cp.Core.app.executeSql(sql)
            'End Select
            '
            ' CalendarEventRules with bad CalendarID
            '
            If False Then
                '
                ' calendar event is the calendar object now
                ' move to a calendar process
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Calendar Event Rules with bad CalendarID.")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete ccCalendarEventRules.*" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendars on ccCalendars.ID=ccCalendarEventRules.CalendarID" _
                            & " WHERE (ccCalendars.ID is null) "
                        Call cp.core.db.executeSql(SQL)
                    Case DataSourceTypeODBCSQLServer
                        SQL = "delete from ccCalendarEventRules" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendars on ccCalendars.ID=ccCalendarEventRules.CalendarID" _
                            & " WHERE (ccCalendars.ID is null) "
                        Call cp.core.db.executeSql(SQL)
                    Case Else
                        SQL = "delete ccCalendarEventRules" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendars on ccCalendars.ID=ccCalendarEventRules.CalendarID" _
                            & " WHERE (ccCalendars.ID is null) "
                        Call cp.core.db.executeSql(SQL)
                End Select
                '
                ' CalendarEventRules with bad CalendarEventID
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Calendar Event Rules with bad CalendarEventID.")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete ccCalendarEventRules.*" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendarEvents on ccCalendarEvents.ID=ccCalendarEventRules.CalendarEventID" _
                            & " WHERE (ccCalendarEvents.ID is null)"
                        Call cp.core.db.executeSql(SQL)
                    Case DataSourceTypeODBCSQLServer
                        SQL = "delete from ccCalendarEventRules" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendarEvents on ccCalendarEvents.ID=ccCalendarEventRules.CalendarEventID" _
                            & " WHERE (ccCalendarEvents.ID is null)"
                        Call cp.core.db.executeSql(SQL)
                    Case Else
                        SQL = "delete ccCalendarEventRules" _
                            & " From ccCalendarEventRules" _
                            & " LEFT JOIN ccCalendarEvents on ccCalendarEvents.ID=ccCalendarEventRules.CalendarEventID" _
                            & " WHERE (ccCalendarEvents.ID is null)"
                        Call cp.core.db.executeSql(SQL)
                End Select
            End If
            '
            ' ContentWatch with bad CContentID
            '     must be deleted manually
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting Content Watch with bad ContentID.")
            SQL = "Select ccContentWatch.ID" _
                & " From ccContentWatch LEFT JOIN ccContent on ccContent.ID=ccContentWatch.ContentID" _
                & " WHERE (ccContent.ID is null)or(ccContent.Active=0)or(ccContent.Active is null)"
            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
            Do While cp.core.db.cs_ok(CS)
                Call cp.core.db.deleteContentRecord("Content Watch", cp.core.db.cs_getInteger(CS, "ID"))
                Call cp.core.db.cs_goNext(CS)
            Loop
            Call cp.core.db.cs_Close(CS)
            '
            ' ContentWatchListRules with bad ContentWatchID
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting ContentWatchList Rules with bad ContentWatchID.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccContentWatchListRules.*" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID" _
                        & " WHERE (ccContentWatch.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete from ccContentWatchListRules" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID" _
                        & " WHERE (ccContentWatch.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccContentWatchListRules" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID" _
                        & " WHERE (ccContentWatch.ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' ContentWatchListRules with bad ContentWatchListID
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting ContentWatchList Rules with bad ContentWatchListID.")
            Select Case DataSourceType
                Case DataSourceTypeODBCAccess
                    SQL = "delete ccContentWatchListRules.*" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID" _
                        & " WHERE (ccContentWatchLists.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case DataSourceTypeODBCSQLServer
                    SQL = "delete from ccContentWatchListRules" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID" _
                        & " WHERE (ccContentWatchLists.ID is null)"
                    Call cp.core.db.executeSql(SQL)
                Case Else
                    SQL = "delete ccContentWatchListRules" _
                        & " From ccContentWatchListRules" _
                        & " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID" _
                        & " WHERE (ccContentWatchLists.ID is null)"
                    Call cp.core.db.executeSql(SQL)
            End Select
            '
            ' Field help with no field
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting field help with no field.")
            SQL = "" _
                & "delete from ccfieldhelp where id in (" _
                & " select h.id" _
                & " from ccfieldhelp h" _
                & " left join ccfields f on f.id=h.fieldid where f.id is null" _
                & ")"
            Call cp.core.db.executeSql(SQL)
            '
            ' Field help duplicates - messy, but I am not sure where they are coming from, and this patchs the edit page performance problem
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Deleting duplicate field help records.")
            SQL = "" _
                & "delete from ccfieldhelp where id in (" _
                & " select b.id" _
                & " from ccfieldhelp a" _
                & " left join ccfieldhelp b on a.fieldid=b.fieldid where a.id< b.id" _
                & ")"
            Call cp.core.db.executeSql(SQL)
            '
            'addon editor rules with no addon
            '
            SQL = "delete from ccAddonContentFieldTypeRules where id in (" _
                & "select r.id from ccAddonContentFieldTypeRules r left join ccaggregatefunctions a on a.id=r.addonid where a.Id Is Null" _
                & ")"
            Call cp.core.db.executeSql(SQL)
            '
            ' convert FieldTypeLongText + htmlContent to FieldTypeHTML
            '
            Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "convert FieldTypeLongText + htmlContent to FieldTypeHTML.")
            SQL = "update ccfields set type=" & FieldTypeIdHTML & " where type=" & FieldTypeIdLongText & " and ( htmlcontent<>0 )"
            Call cp.core.db.executeSql(SQL)
            ''
            '' convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile
            ''
            'Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile.")
            'SQL = "update ccfields set type=" & FieldTypeIdFileHTMLPrivate & " where type=" & FieldTypeIdFileTextPrivate & " and ( htmlcontent<>0 )"
            'Call cp.core.app.executeSql(SQL)
            '
            ' Log files Older then 30 days
            '
            Call HouseKeep_App_Daily_LogFolder("temp", thirtyDaysAgo)
            Call HouseKeep_App_Daily_LogFolder("TrapLogs", thirtyDaysAgo)
            Call HouseKeep_App_Daily_LogFolder("BounceLog", thirtyDaysAgo)
            Call HouseKeep_App_Daily_LogFolder("BounceProcessing", thirtyDaysAgo)
            Call HouseKeep_App_Daily_LogFolder("SMTPLog", thirtyDaysAgo)
            Call HouseKeep_App_Daily_LogFolder("DebugLog", thirtyDaysAgo)
            '
            ' Content TextFile types with no controlling record
            '
            If EncodeBoolean(cp.core.siteProperties.getText("ArchiveAllowFileClean", "false")) Then
                '
                Dim DSType As Integer
                DSType = cp.core.db.getDataSourceType("")
                Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "Content TextFile types with no controlling record.")
                SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName" _
                    & " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID" _
                    & " Where (((ccFields.Type) = 10))" _
                    & " ORDER BY ccTables.Name"
                CS = cp.core.db.cs_openCsSql_rev("Default", SQL)
                Do While cp.core.db.cs_ok(CS)
                    '
                    ' Get all the files in this path, and check that the record exists with this in its field
                    '
                    FieldName = cp.core.db.cs_getText(CS, "FieldName")
                    TableName = cp.core.db.cs_getText(CS, "TableName")
                    PathName = TableName & "\" & FieldName
                    FileList = cp.core.cdnFiles.getFileList(PathName)
                    If FileList.Count > 0 Then
                        On Error Resume Next
                        SQL = "CREATE INDEX temp" & FieldName & " ON " & TableName & " (" & FieldName & ")"
                        Call cp.core.db.executeSql(SQL)
                        On Error GoTo ErrorTrap
                        For Each file As IO.FileInfo In FileList
                            Filename = file.Name
                            VirtualFileName = PathName & "\" & Filename
                            VirtualLink = vbReplace(VirtualFileName, "\", "/")
                            FileSize = file.Length
                            If FileSize = 0 Then
                                SQL = "update " & TableName & " set " & FieldName & "=null where (" & FieldName & "=" & cp.core.db.encodeSQLText(VirtualFileName) & ")or(" & FieldName & "=" & cp.core.db.encodeSQLText(VirtualLink) & ")"
                                Call cp.core.db.executeSql(SQL)
                                Call cp.core.cdnFiles.deleteFile(VirtualFileName)
                            Else
                                SQL = "SELECT ID FROM " & TableName & " WHERE (" & FieldName & "=" & cp.core.db.encodeSQLText(VirtualFileName) & ")or(" & FieldName & "=" & cp.core.db.encodeSQLText(VirtualLink) & ")"
                                CSTest = cp.core.db.cs_openCsSql_rev("default", SQL)
                                If Not cp.core.db.cs_ok(CSTest) Then
                                    Call cp.core.cdnFiles.deleteFile(VirtualFileName)
                                End If
                                Call cp.core.db.cs_Close(CSTest)
                            End If
                        Next
                        If DSType = 1 Then
                            ' access
                            SQL = "Drop INDEX temp" & FieldName & " ON " & TableName
                        ElseIf DSType = 2 Then
                            ' sql server
                            SQL = "DROP INDEX " & TableName & ".temp" & FieldName
                        Else
                            ' mysql
                            SQL = "ALTER TABLE " & TableName & " DROP INDEX temp" & FieldName
                        End If
                        Call cp.core.db.executeSql(SQL)
                        On Error GoTo ErrorTrap
                    End If
                    Call cp.core.db.cs_goNext(CS)
                Loop
                Call cp.core.db.cs_Close(CS)
                '
                ' problem here is 1) images may have resized images in the folder
                ' 2) files may be in the wrong recordID if workflow.
                '
                '        '
                '        ' Content File types with no controlling record
                '        '
                '        SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName" _
                '            & " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID" _
                '            & " Where ((ccFields.Type=6)OR(ccFields.Type=11)OR(ccFields.Type=16)OR(ccFields.Type=17)OR(ccFields.Type=18))" _
                '            & " ORDER BY ccTables.Name"
                '        CS = cp.Core.app.csv_OpenCSSQL("Default", SQL)
                '        Do While cp.Core.app.csv_IsCSOK(CS)
                '            '
                '            ' Get all the files in this path, and check that the record exists with this in its field
                '            '
                '            FieldName = cp.Core.app.csv_cs_getText(CS, "FieldName")
                '            TableName = cp.Core.app.csv_cs_getText(CS, "TableName")
                '            If cp.Core.csv_IsSQLTableField("Default", TableName, FieldName) Then
                '                PathName = TableName & "\" & FieldName
                '                PathNameRev = TableName & "/" & FieldName
                '                FolderList = cp.Core.contentFiles.getFolderList(PathName)
                '                If FolderList <> "" Then
                '                    FolderArray = Split(FolderList, vbCrLf)
                '                    FolderArrayCount = UBound(FolderArray) + 1
                '                    For FolderArrayPointer = 0 To FolderArrayCount - 1
                '                        If FolderArray(FolderArrayPointer) <> "" Then
                '                            FolderSplit = Split(FolderArray(FolderArrayPointer), ",")
                '                            FolderName = FolderSplit(0)
                '                            '
                '                            ' just verify the record exists -- all files in the folder are valid
                '                            '
                '                            SQL = "select id from " & TableName & " where id=" & encodeSQLNumber(FolderName)
                '                            CSTest = cp.Core.app.csv_OpenCSSQL("default", SQL)
                '                            If Not cp.Core.app.csv_IsCSOK(CSTest) Then
                '                            '    Call cp.Core.csv_DeleteVirtualFolder(PathNameRev & "\" & FolderName)
                '                            End If
                '                            Call cp.Core.app.csv_CloseCS(CSTest)
                '
                '                            FileList = cp.Core.csv_GetVirtualFileList(PathName & "\" & FolderName)
                '                            If FileList <> "" Then
                '                                FileArray = Split(FileList, vbCrLf)
                '                                FileArrayCount = UBound(FileArray) + 1
                '                                For FileArrayPointer = 0 To FileArrayCount - 1
                '                                    If FileArray(FileArrayPointer) <> "" Then
                '                                        FileSplit = Split(FileArray(FileArrayPointer), ",")
                '                                        FilenameOriginal = FileSplit(0)
                '                                        Filename = FilenameOriginal
                '                                        FilenameAltSize = ""
                '                                        Pos = InStrRev(Filename, ".")
                '
                '                                        If Pos > 0 Then
                '                                            FilenameExt = Mid(Filename, Pos + 1)
                '                                            FilenameNoExt = Mid(Filename, 1, Pos - 1)
                '                                            Pos = InStrRev(FilenameNoExt, "-")
                '                                            If Pos > 0 Then
                '                                                FilenameAltSize = Mid(FilenameNoExt, Pos + 1)
                '                                                If FilenameAltSize <> "" Then
                '                                                FilenameDim = Split(FilenameAltSize, "x")
                '                                                If UBound(FilenameDim) = 1 Then
                '                                                    If vbIsNumeric(FilenameDim(0)) And vbIsNumeric(FilenameDim(1)) Then
                '                                                        FilenameNoExt = Mid(FilenameNoExt, 1, Pos - 1)
                '                                                    End If
                '                                                End If
                '                                                End If
                '                                            End If
                '                                            Filename = FilenameNoExt & "." & FilenameExt
                '                                        End If
                '                                        If FilenameAltSize <> "" Then
                '                                            SQL = "SELECT ID FROM " & TableName & " WHERE (" & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & FilenameOriginal) & ")or(" & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & Filename) & ")"
                '                                        Else
                '                                            SQL = "SELECT ID FROM " & TableName & " WHERE " & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & FilenameOriginal)
                '                                        End If
                '                                        CSTest = cp.Core.app.csv_OpenCSSQL("default", SQL)
                '                                        If Not cp.Core.app.csv_IsCSOK(CSTest) Then
                '                                            Call cp.Core.virtualFiles.DeleteFile(PathNameRev & "/" & FolderName & "/" & FilenameOriginal)
                '                                        End If
                '                                        Call cp.Core.app.csv_CloseCS(CSTest)
                '                                    End If
                '                                Next
                '                            End If
                '                        End If
                '                    Next
                '                End If
                '            End If
                '            Call cp.Core.app.csv_NextCSRecord(CS)
                '        Loop
                '        Call cp.Core.app.csv_CloseCS(CS)
            End If
            cp.core.db.sqlCommandTimeout = TimeoutSave
            Exit Sub
            '
ErrorTrap:
            Call HandleClassTrapError(appName, "HouseKeep_App_Daily", "Trap", True)
            Err.Clear()
            If TimeoutSave <> 0 Then
                cp.core.db.sqlCommandTimeout = TimeoutSave
            End If
        End Sub
        '
        '
        '
        Private Sub HouseKeep_App_Daily_RemoveVisitRecords(ByVal DeleteBeforeDate As Date, ByVal DataSourceType As Integer)
            On Error GoTo ErrorTrap
            '
            Dim TimeoutSave As Integer
            'Dim Controller As controlClass
            Dim VisitArchiveAgeDays As Integer
            Dim VisitArchiveDeleteSize As Integer
            ''Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            '      Dim CSConnection As appEnvironmentStruc
            Dim SQL As String
            Dim SQLCriteria As String
            Dim PathName As String
            Dim TableName As String
            Dim FieldName As String
            Dim FileList As String
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim CS As Integer
            Dim CSTest As Integer
            Dim Filename As String
            Dim FileSplit() As String
            Dim FolderName As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            Dim AdminLicenseCount As Integer
            Dim DeleteBeforeDateSQL As String
            Dim appName As String
            Dim SQLTablePeople As String
            'Dim SQLTableMemberRules As String
            'Dim SQLTableGroups As String
            'Dim PeopleCID as integer
            Dim DefaultName As String
            '
            ' Set long timeout (30 min) needed for heavy work on big tables
            '
            TimeoutSave = cp.core.db.sqlCommandTimeout
            cp.core.db.sqlCommandTimeout = 1800
            '
            SQLTablePeople = cp.core.metaData.getContentTablename("People")
            'SQLTableMemberRules = cp.Core.csv_GetContentTablename("Member Rules")
            'SQLTableGroups = cp.Core.csv_GetContentTablename("Groups")
            '
            'VisitArchiveAgeDays = encodeInteger(cp.Core.csv_GetSiteProperty("ArchiveRecordAgeDays", "0"))
            If True Then
                appName = cp.core.serverConfig.appConfig.name
                DeleteBeforeDateSQL = cp.core.db.encodeSQLDate(DeleteBeforeDate)
                '
                ' Visits older then archive age
                '
                Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep_App_Daily_RemoveVisitRecords(" & appName & ")", "Deleting visits before [" & DeleteBeforeDateSQL & "]")
                Call cp.core.DeleteTableRecordChunks("default", "ccVisits", "(DateAdded<" & DeleteBeforeDateSQL & ")", 1000, 10000)
                '
                ' Viewings with visits before the first
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily_RemoveVisitRecords(" & appName & ")", "Deleting viewings with visitIDs lower then the lowest ccVisits.ID")
                Call cp.core.DeleteTableRecordChunks("default", "ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000)
                '
                ' Visitors with no visits
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily_RemoveVisitRecords(" & appName & ")", "Deleting visitors with no visits")
                Select Case DataSourceType
                    Case DataSourceTypeODBCAccess
                        SQL = "delete ccVisitors.*" _
                            & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                            & " where ccVisits.ID is null"
                        Call cp.core.db.executeSql(SQL)

                    Case DataSourceTypeODBCSQLServer
                        SQL = "delete From ccVisitors" _
                            & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                            & " where ccVisits.ID is null"
                        Call cp.core.db.executeSql(SQL)
                    Case Else
                        SQL = "delete ccVisitors" _
                            & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                            & " where ccVisits.ID is null"
                        Call cp.core.db.executeSql(SQL)
                End Select
                '        '
                '        ' Delete People
                '        '   created before DeleteBeforeDate,
                '        '   created during a visit (not created by another process),
                '        '   with default name (created during a hit)
                '        '   with no username (they are not planning on returning)
                '        '   with 1 visit (not created with 0 visits, has not returned)
                '        '
                '        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveVisitRecords(" & AppName & ")", "Deleting members with default name [" & DefaultName & "], LastVisit before DeleteBeforeDate [" & DeleteBeforeDate & "], exactly one total visit, a null username and a null email address.")
                '        SQLCriteria = "" _
                '            & "(" & SQLTablePeople & ".Name=" & encodeSQLText(DefaultName) & ")" _
                '            & " and(LastVisit<" & DeleteBeforeDateSQL & ")" _
                '            & " and(createdbyvisit=1)" _
                '            & " and(Visits=1)" _
                '            & " and(Username is null)" _
                '            & " and(email is null)"
                '        Call cp.Core.csv_DeleteTableRecordChunks("default", "" & SQLTablePeople & "", SQLCriteria, 1000, 10000)
            End If
            '
            ' restore sved timeout
            '
            cp.core.db.sqlCommandTimeout = TimeoutSave
            Exit Sub
            '
ErrorTrap:
            Call HandleClassTrapError(appName, "HouseKeep_App_Daily_RemoveVisitRecords", "Trap", True)
            Err.Clear()
            '
            ' restore saved timeout
            '
            If TimeoutSave <> 0 Then
                cp.core.db.sqlCommandTimeout = TimeoutSave
            End If
        End Sub
        '
        '
        '
        Private Sub HouseKeep_App_Daily_RemoveGuestRecords(ByVal DeleteBeforeDate As Date, ByVal DataSourceType As Integer)
            On Error GoTo ErrorTrap
            '
            Dim TimeoutSave As Integer
            'Dim Controller As controlClass
            Dim VisitArchiveAgeDays As Integer
            Dim VisitArchiveDeleteSize As Integer
            ''Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            '   Dim CSConnection As appEnvironmentStruc
            Dim SQL As String
            Dim SQLCriteria As String
            Dim PathName As String
            Dim TableName As String
            Dim FieldName As String
            Dim FileList As String
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim CS As Integer
            Dim CSTest As Integer
            Dim Filename As String
            Dim FileSplit() As String
            Dim FolderName As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            Dim AdminLicenseCount As Integer
            Dim DeleteBeforeDateSQL As String
            Dim appName As String
            Dim SQLTablePeople As String
            'Dim SQLTableMemberRules As String
            'Dim SQLTableGroups As String
            'Dim PeopleCID as integer
            Dim DefaultName As String
            '
            ' Set long timeout (30 min) needed for heavy work on big tables
            '
            TimeoutSave = cp.core.db.sqlCommandTimeout
            cp.core.db.sqlCommandTimeout = 1800
            '
            SQLTablePeople = cp.core.metaData.getContentTablename("People")
            'SQLTableMemberRules = cp.Core.csv_GetContentTablename("Member Rules")
            'SQLTableGroups = cp.Core.csv_GetContentTablename("Groups")
            '
            'VisitArchiveAgeDays = encodeInteger(cp.Core.csv_GetSiteProperty("ArchiveRecordAgeDays", "0"))
            If True Then
                appName = cp.core.serverConfig.appConfig.name
                DeleteBeforeDateSQL = cp.core.db.encodeSQLDate(DeleteBeforeDate)
                '        '
                '        ' Visits older then archive age
                '        '
                '        Call AppendClassLog(cp.Core.appEnvironment.name, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting visits before [" & DeleteBeforeDateSQL & "]")
                '        Call cp.Core.csv_DeleteTableRecordChunks("default", "ccVisits", "(DateAdded<" & DeleteBeforeDateSQL & ")", 1000, 10000)
                '        '
                '        ' Viewings with visits before the first
                '        '
                '        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting viewings with visitIDs lower then the lowest ccVisits.ID")
                '        Call cp.Core.csv_DeleteTableRecordChunks("default", "ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000)
                '        '
                '        ' Visitors with no visits
                '        '
                '        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting visitors with no visits")
                '        Select Case DataSourceType
                '            Case DataSourceTypeODBCAccess
                '                SQL = "delete ccVisitors.*" _
                '                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                '                    & " where ccVisits.ID is null"
                '                Call cp.Core.app.executeSql(sql)
                '
                '            Case DataSourceTypeODBCSQLServer
                '                SQL = "delete From ccVisitors" _
                '                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                '                    & " where ccVisits.ID is null"
                '                Call cp.Core.app.executeSql(sql)
                '            Case Else
                '                SQL = "delete ccVisitors" _
                '                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                '                    & " where ccVisits.ID is null"
                '                Call cp.Core.app.executeSql(sql)
                '        End Select
                '
                ' Delete People
                '   created before DeleteBeforeDate,
                '   created during a visit (not created by another process),
                '   x with default name (created during a hit) - no, spider detect changes name
                '   with no username (they are not planning on returning)
                '   with 1 visit (not created with 0 visits, has not returned)
                '
                Call AppendClassLog(appName, "HouseKeep_App_Daily_RemoveGuestRecords(" & appName & ")", "Deleting members with  LastVisit before DeleteBeforeDate [" & DeleteBeforeDate & "], exactly one total visit, a null username and a null email address.")
                SQLCriteria = "" _
                    & " (LastVisit<" & DeleteBeforeDateSQL & ")" _
                    & " and(createdbyvisit=1)" _
                    & " and(Visits=1)" _
                    & " and(Username is null)" _
                    & " and(email is null)"
                Call cp.core.DeleteTableRecordChunks("default", "" & SQLTablePeople & "", SQLCriteria, 1000, 10000)
                '& "(" & SQLTablePeople & ".Name=" & encodeSQLText(DefaultName) & ")"
            End If
            '
            ' restore sved timeout
            '
            cp.core.db.sqlCommandTimeout = TimeoutSave
            Exit Sub
            '
ErrorTrap:
            Call HandleClassTrapError(appName, "HouseKeep_App_Daily_RemoveGuestRecords", "Trap", True)
            Err.Clear()
            '
            ' restore saved timeout
            '
            If TimeoutSave <> 0 Then
                cp.core.db.sqlCommandTimeout = TimeoutSave
            End If
        End Sub
        '
        '=========================================================================================
        ' Summarize the visits
        '   excludes non-cookie visits
        '   excludes administrator and developer visits
        '   excludes authenticated users with ExcludeFromReporting
        '
        ' Average time on site
        '
        '   Example data
        '   Pages       TimeToLastHit
        '   1           0           - hit 1 page, start time = last time
        '   10          3510        - hit 10 pages, first hit time - last hit time = 3510
        '   2           30          - hit 2 pages, first hit time - last hit time = 30
        '
        ' AveReadTime is the average time spent reading pages
        '   this is calculated from the multi-page visits only
        '   = MultiPageTimeToLastHitSum / ( MultiPageHitCnt - MultiPageVisitCnt )
        '   = ( 3510 + 30 ) / ((10+2) - 2 )
        '   = 354
        '
        ' TotalTimeOnSite is the total time people spent reading pages
        '   There are two parts:
        '     1) the TimeToLastHit, which covers all but the last hit of each visit
        '     2) assume the last hit of each visit is the AveReadTime
        '   = MultiPageTimeToLastHitSum + ( AveReadTime * VisitCnt )
        '   = ( 3510 + 30 ) + ( 354 * 3 )
        '   = 4602
        '
        ' AveTimeOnSite
        '   = TotalTimeOnSite / TotalHits
        '   = 4602 / 3
        '   = 1534
        '
        '=========================================================================================
        '
        Public Sub HouseKeep_VisitSummary(ByVal StartTimeDate As Date, ByVal EndTimeDate As Date, ByVal HourDuration As Integer, ByVal BuildVersion As String, ByVal OldestVisitSummaryWeCareAbout As Date)
            '
            On Error GoTo ErrorTrap
            '
            'Dim StartDate As Date
            Dim StartTimeHoursSinceMidnight As Double
            Dim PeriodStart As Date
            Dim TotalTimeOnSite
            Dim MultiPageVisitCnt As Integer
            Dim MultiPageHitCnt As Integer
            Dim MultiPageTimetoLastHitSum As Double
            Dim TimeOnSite As Double
            'Dim PeriodStepInHours As Double
            Dim PeriodDatePtr As Date
            Dim StartOfHour As Date
            Dim DateNumber As Integer
            Dim TimeNumber As Integer
            Dim SumStartTime As Double
            Dim SumStopTime As Double
            Dim HoursPerDay As Integer
            Dim DateStart As Date
            Dim DateEnd As Date
            Dim NewVisitorVisits As Integer
            Dim SinglePageVisits As Integer
            Dim AuthenticatedVisits As Integer
            Dim MobileVisits As Integer
            Dim BotVisits As Integer
            Dim NoCookieVisits As Integer
            Dim AveTimeOnSite As Double
            Dim HitCnt As Integer
            Dim VisitCnt As Integer
            Dim OldestDateAdded As Date
            Dim EmptyVariant As Object
            Dim NeedToClearCache As Boolean
            Dim ArchiveParentID As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim LoopPtr As Integer
            Dim Ptr As Integer
            Dim LocalFile As String
            Dim LocalFilename As String
            Dim Folders() As String
            Dim FolderCnt As Integer
            Dim CollectionGUID As String
            Dim CollectionName As String
            Dim Pos As Integer
            Dim LastChangeDate As String
            Dim SubFolderList As String
            Dim SubFolders() As String
            Dim SubFolder As String
            Dim Cnt As Integer
            Dim LocalGUID As String
            Dim LocalLastChangeDateStr As String
            Dim LocalLastChangeDate As Date
            Dim LibGUID As String
            Dim LibLastChangeDateStr As String
            Dim LibLastChangeDate As Date
            Dim LibListNode As XmlNode
            Dim LocalListNode As XmlNode
            Dim CollectionNode As XmlNode
            Dim LibraryCollections As New XmlDocument
            Dim LocalCollections As New XmlDocument
            Dim Doc As New XmlDocument
            ''Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            Dim SetTimeCheckString As String
            Dim SetTimeCheck As Double
            Dim LogDate As Date
            Dim FolderName As String
            Dim FileList As String
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim FileSplit() As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            'Dim fs As New fileSystemClass
            Dim VisitArchiveAgeDays As Integer
            Dim NewDay As Boolean
            Dim NewHour As Boolean
            '
            Dim LastTimeCheck As Date
            '
            Dim ConfigFilename As String
            Dim Config As String
            Dim ConfigLines() As String
            '
            Dim Line As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            Dim NameValue() As String
            Dim SQLNow As String
            Dim SQL As String
            Dim AveReadTime As Integer
            'Dim AddonInstall As New addonInstallClass
            '
            If BuildVersion < cp.Version Then
                Call HandleClassInternalError(cp.core.serverConfig.appConfig.name, "HouseKeep_VisitSummary", ignoreInteger, "Can not summarize analytics until this site's data needs been upgraded.")
            Else
                PeriodStart = StartTimeDate
                If PeriodStart < OldestVisitSummaryWeCareAbout Then
                    PeriodStart = OldestVisitSummaryWeCareAbout
                End If
                StartTimeHoursSinceMidnight = PeriodStart.TimeOfDay.TotalHours
                PeriodStart = PeriodStart.Date.AddHours(StartTimeHoursSinceMidnight)
                'PeriodStepInHours = CDbl(HourDuration) / 24.0!
                PeriodDatePtr = PeriodStart
                Do While PeriodDatePtr < EndTimeDate
                    '
                    DateNumber = Int(PeriodDatePtr.AddHours(HourDuration / 2).ToOADate)
                    TimeNumber = PeriodDatePtr.TimeOfDay.TotalHours
                    DateStart = PeriodDatePtr.Date
                    DateEnd = PeriodDatePtr.AddHours(HourDuration).Date
                    '
                    VisitCnt = 0
                    HitCnt = 0
                    SumStartTime = 0
                    SumStopTime = 0
                    NewVisitorVisits = 0
                    SinglePageVisits = 0
                    MultiPageVisitCnt = 0
                    MultiPageTimetoLastHitSum = 0
                    AuthenticatedVisits = 0
                    MobileVisits = 0
                    BotVisits = 0
                    NoCookieVisits = 0
                    AveTimeOnSite = 0
                    '
                    ' No Cookie Visits
                    '
                    SQL = "select count(v.id) as NoCookieVisits" _
                        & " from ccvisits v" _
                        & " where (v.CookieSupport<>1)" _
                        & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                        & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                        & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                        & ""
                    'SQL = "select count(id) as NoCookieVisits from ccvisits where (CookieSupport<>1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                    CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                    If cp.core.db.cs_ok(CS) Then
                        NoCookieVisits = cp.core.db.cs_getInteger(CS, "NoCookieVisits")
                    End If
                    Call cp.core.db.cs_Close(CS)
                    '
                    ' Total Visits
                    '
                    SQL = "select count(v.id) as VisitCnt ,Sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimeOnSite" _
                        & " from ccvisits v" _
                        & " where (v.CookieSupport<>0)" _
                        & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                        & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                        & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                        & ""
                    'SQL = "select count(id) as VisitCnt ,Sum(PageVisits) as HitCnt ,sum(TimetoLastHit) as TimeOnSite from ccvisits where (CookieSupport<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and (dateadded<" & encodeSQLDate(DateEnd) & ")"
                    CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                    If cp.core.db.cs_ok(CS) Then
                        VisitCnt = cp.core.db.cs_getInteger(CS, "VisitCnt")
                        HitCnt = cp.core.db.cs_getInteger(CS, "HitCnt")
                        TimeOnSite = cp.core.db.cs_getNumber(CS, "TimeOnSite")
                    End If
                    Call cp.core.db.cs_Close(CS)
                    '
                    ' Visits by new visitors
                    '
                    If VisitCnt > 0 Then
                        SQL = "select count(v.id) as NewVisitorVisits" _
                            & " from ccvisits v" _
                            & " where (v.CookieSupport<>0)" _
                            & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                            & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                            & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                            & " and(v.VisitorNew<>0)" _
                            & ""
                        'SQL = "select count(id) as NewVisitorVisits from ccvisits where (CookieSupport<>0)and(VisitorNew<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                        If cp.core.db.cs_ok(CS) Then
                            NewVisitorVisits = cp.core.db.cs_getInteger(CS, "NewVisitorVisits")
                        End If
                        Call cp.core.db.cs_Close(CS)
                        '
                        ' Single Page Visits
                        '
                        SQL = "select count(v.id) as SinglePageVisits" _
                            & " from ccvisits v" _
                            & " where (v.CookieSupport<>0)" _
                            & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                            & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                            & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                            & " and(v.PageVisits=1)" _
                            & ""
                        'SQL = "select count(id) as SinglePageVisits from ccvisits where (CookieSupport<>0)and(PageVisits=1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                        If cp.core.db.cs_ok(CS) Then
                            SinglePageVisits = cp.core.db.cs_getInteger(CS, "SinglePageVisits")
                        End If
                        Call cp.core.db.cs_Close(CS)
                        '
                        ' Multipage Visits
                        '
                        SQL = "select count(v.id) as VisitCnt ,sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimetoLastHitSum " _
                            & " from ccvisits v" _
                            & " where (v.CookieSupport<>0)" _
                            & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                            & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                            & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                            & " and(PageVisits>1)" _
                            & ""
                        'SQL = "select count(id) as VisitCnt ,sum(PageVisits) as HitCnt ,sum(TimetoLastHit) as TimetoLastHitSum from ccvisits where (CookieSupport<>0)and(PageVisits>1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and (dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                        If cp.core.db.cs_ok(CS) Then
                            MultiPageVisitCnt = cp.core.db.cs_getInteger(CS, "VisitCnt")
                            MultiPageHitCnt = cp.core.db.cs_getInteger(CS, "HitCnt")
                            MultiPageTimetoLastHitSum = cp.core.db.cs_getNumber(CS, "TimetoLastHitSum")
                        End If
                        Call cp.core.db.cs_Close(CS)
                        '
                        ' Authenticated Visits
                        '
                        SQL = "select count(v.id) as AuthenticatedVisits " _
                            & " from ccvisits v" _
                            & " where (v.CookieSupport<>0)" _
                            & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                            & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                            & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                            & " and(VisitAuthenticated<>0)" _
                            & ""
                        'SQL = "select count(id) as AuthenticatedVisits from ccvisits where (CookieSupport<>0)and(VisitAuthenticated<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                        If cp.core.db.cs_ok(CS) Then
                            AuthenticatedVisits = cp.core.db.cs_getInteger(CS, "AuthenticatedVisits")
                        End If
                        Call cp.core.db.cs_Close(CS)
                        '
                        If True Then
                            '
                            ' Mobile Visits
                            '
                            SQL = "select count(v.id) as cnt " _
                                & " from ccvisits v" _
                                & " where (v.CookieSupport<>0)" _
                                & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                                & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                                & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                                & " and(Mobile<>0)" _
                                & ""
                            'SQL = "select count(id) as AuthenticatedVisits from ccvisits where (CookieSupport<>0)and(VisitAuthenticated<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                            If cp.core.db.cs_ok(CS) Then
                                MobileVisits = cp.core.db.cs_getInteger(CS, "cnt")
                            End If
                            Call cp.core.db.cs_Close(CS)
                            '
                            ' Bot Visits
                            '
                            SQL = "select count(v.id) as cnt " _
                                & " from ccvisits v" _
                                & " where (v.CookieSupport<>0)" _
                                & " and(v.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                                & " and (v.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                                & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                                & " and(Bot<>0)" _
                                & ""
                            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                            If cp.core.db.cs_ok(CS) Then
                                BotVisits = cp.core.db.cs_getInteger(CS, "cnt")
                            End If
                            Call cp.core.db.cs_Close(CS)
                        End If
                        '
                        If (MultiPageHitCnt > MultiPageVisitCnt) And (HitCnt > 0) Then
                            AveReadTime = MultiPageTimetoLastHitSum / (MultiPageHitCnt - MultiPageVisitCnt)
                            TotalTimeOnSite = MultiPageTimetoLastHitSum + (AveReadTime * VisitCnt)
                            AveTimeOnSite = TotalTimeOnSite / VisitCnt
                        End If
                    End If
                    '
                    ' Add or update the Visit Summary Record
                    '
                    CS = cp.core.db.cs_open("Visit Summary", "(timeduration=" & HourDuration & ")and(DateNumber=" & DateNumber & ")and(TimeNumber=" & TimeNumber & ")")
                    If Not cp.core.db.cs_ok(CS) Then
                        Call cp.core.db.cs_Close(CS)
                        CS = cp.core.db.cs_insertRecord("Visit Summary", 0)
                    End If
                    '
                    If cp.core.db.cs_ok(CS) Then
                        Call cp.core.db.cs_set(CS, "name", HourDuration & " hr summary for " & Date.FromOADate(DateNumber).ToShortDateString & " " & TimeNumber & ":00")
                        Call cp.core.db.cs_set(CS, "DateNumber", DateNumber)
                        Call cp.core.db.cs_set(CS, "TimeNumber", TimeNumber)
                        Call cp.core.db.cs_set(CS, "Visits", VisitCnt)
                        Call cp.core.db.cs_set(CS, "PagesViewed", HitCnt)
                        Call cp.core.db.cs_set(CS, "TimeDuration", HourDuration)
                        Call cp.core.db.cs_set(CS, "NewVisitorVisits", NewVisitorVisits)
                        Call cp.core.db.cs_set(CS, "SinglePageVisits", SinglePageVisits)
                        Call cp.core.db.cs_set(CS, "AuthenticatedVisits", AuthenticatedVisits)
                        Call cp.core.db.cs_set(CS, "NoCookieVisits", NoCookieVisits)
                        Call cp.core.db.cs_set(CS, "AveTimeOnSite", AveTimeOnSite)
                        If True Then
                            Call cp.core.db.cs_set(CS, "MobileVisits", MobileVisits)
                            Call cp.core.db.cs_set(CS, "BotVisits", BotVisits)
                        End If
                    End If
                    Call cp.core.db.cs_Close(CS)
                    PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration)
                Loop
            End If
            '
            Exit Sub
ErrorTrap:
            Call HandleClassTrapError(cp.core.serverConfig.appConfig.name, "HouseKeep_VisitSummary", "Trap", True)
            Err.Clear()
        End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Public Sub AppendClassLog(ByVal ApplicationName As String, ByVal MethodName As String, ByVal LogCopy As String)
            On Error GoTo ErrorTrap
            '
            cp.core.appendLogWithLegacyRow(ApplicationName, LogCopy, "ccHouseKeep", "HouseKeepClass", MethodName, 0, "", "", False, True, "", "HouseKeep", "")
            '
            Exit Sub
            '
ErrorTrap:
            Err.Clear()
        End Sub
        '
        '
        '
        Private Sub HousekeepLogFolder(ByVal appName As String, ByVal FolderName As String)
            On Error GoTo ErrorTrap
            '
            Dim LogDate As Date
            'Dim fs As New fileSystemClass
            Dim FileList As IO.FileInfo()
            '
            LogDate = DateTime.Now.AddDays(-30)
            Call AppendClassLog("", "HouseKeep", "Deleting Logs [" & FolderName & "] older than 30 days")
            FileList = cp.core.programDataFiles.getFileList(FolderName)
            For Each file As IO.FileInfo In FileList
                If file.CreationTime < LogDate Then
                    cp.core.privateFiles.deleteFile(FolderName & "\" & file.Name)
                End If
            Next
            '
            Exit Sub
            '
ErrorTrap:
            Call HandleClassTrapError(appName, "HouseKeepLogFolder", "Trap", True)
            Err.Clear()
        End Sub
        '
        Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal MethodName As String, ByVal Cause As String, ByVal ResumeNext As Boolean)
            '
            cp.core.handleLegacyError3(ApplicationName, Cause, "ccHouseKeep", "HouseKeepClass", MethodName, Err.Number, Err.Source, Err.Description, True, ResumeNext, "")
            '
        End Sub
        '
        ' ----- temp solution to convert error reporting without spending the time right now
        '
        Private Sub HandleClassInternalError(ByVal ApplicationName As String, ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal Cause As String)
            '
            cp.core.handleLegacyError3(ApplicationName, Cause, "ccHouseKeep", "HouseKeepClass", MethodName, ErrNumber, "App.EXEName", Cause, False, True, "")
            '
        End Sub
        '
        '
        '
        Private Sub HouseKeep_App_Daily_LogFolder(ByVal FolderName As String, ByVal LastMonth As Date)
            On Error GoTo ErrorTrap
            '
            Dim FileList As IO.FileInfo()
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim FileSplit() As String
            '
            Call AppendClassLog(cp.core.serverConfig.appConfig.name, "HouseKeep_App_Daily_LogFolder(" & cp.core.serverConfig.appConfig.name & ")", "Deleting files from folder [" & FolderName & "] older than " & LastMonth)
            FileList = cp.core.privateFiles.getFileList(FolderName)
            For Each file As IO.FileInfo In FileList
                If file.CreationTime < LastMonth Then
                    Call cp.core.privateFiles.deleteFile(FolderName & "/" & file.Name)
                End If
            Next
            Exit Sub
            '
ErrorTrap:
            Call HandleClassTrapError(cp.core.serverConfig.appConfig.name, "HouseKeepLogFolder", "Trap", True)
            Err.Clear()
        End Sub
        '
        '
        '
        Private Function DownloadUpdates() As Boolean
            Dim loadOK As Boolean = True
            Try
                Dim Doc As XmlDocument
                Dim CDefSection As XmlNode
                Dim URL As String
                Dim Copy As String
                '
                Doc = New XmlDocument
                URL = "http://support.contensive.com/GetUpdates?iv=" & cp.Version()
                loadOK = True
                Call Doc.Load(URL)
                With Doc.DocumentElement
                    If (LCase(Doc.DocumentElement.Name) <> vbLCase("ContensiveUpdate")) Then
                        DownloadUpdates = False
                    Else
                        If Doc.DocumentElement.ChildNodes.Count = 0 Then
                            DownloadUpdates = False
                        Else
                            With Doc.DocumentElement
                                For Each CDefSection In .ChildNodes
                                    Copy = CDefSection.InnerText
                                    Select Case vbLCase(CDefSection.Name)
                                        Case "mastervisitnamelist"
                                            '
                                            ' Read in the interfaces and save to Add-ons
                                            '
                                            Call cp.core.privateFiles.saveFile("config\VisitNameList.txt", Copy)
                                            'Call cp.Core.app.privateFiles.SaveFile(getAppPath & "\config\DefaultBotNameList.txt", copy)
                                        Case "masteremailbouncefilters"
                                            '
                                            ' save the updated filters file
                                            '
                                            Call cp.core.privateFiles.saveFile("config\EmailBounceFilters.txt", Copy)
                                            'Call cp.Core.app.privateFiles.SaveFile(getAppPath & "\cclib\config\Filters.txt", copy)
                                        Case "mastermobilebrowserlist"
                                            '
                                            ' save the updated filters file
                                            '
                                            Call cp.core.privateFiles.saveFile("config\MobileBrowserList.txt", Copy)
                                    End Select
                                Next
                            End With
                        End If
                    End If
                End With
            Catch ex As Exception
                '
                ' error - Need a way to reach the user that submitted the file
                '
                DownloadUpdates = False
                Call HandleClassTrapError("", "DownloadUpdates", "GetUpdates returned error", True)
                loadOK = False
            End Try
            Return loadOK
        End Function
        '
        '=========================================================================================
        ' Summarize the page views
        '   excludes non-cookie visits
        '   excludes administrator and developer visits
        '   excludes authenticated users with ExcludeFromReporting
        '
        '=========================================================================================
        '
        Public Sub HouseKeep_PageViewSummary(ByVal StartTimeDate As Date, ByVal EndTimeDate As Date, ByVal HourDuration As Integer, ByVal BuildVersion As String, ByVal OldestVisitSummaryWeCareAbout As Date)
            '
            On Error GoTo ErrorTrap
            '
            Dim baseCriteria As String
            Dim StartDate As Date
            'Dim StartTime As Date
            Dim PeriodStart As Date
            Dim TotalTimeOnSite
            Dim MultiPageVisitCnt As Integer
            Dim MultiPageHitCnt As Integer
            Dim MultiPageTimetoLastHitSum As Double
            Dim TimeOnSite As Double
            Dim PeriodStep As Double
            Dim PeriodDatePtr As Date
            Dim StartOfHour As Date
            Dim DateNumber As Integer
            Dim TimeNumber As Integer
            Dim SumStartTime As Double
            Dim SumStopTime As Double
            Dim HoursPerDay As Integer
            Dim DateStart As Date
            Dim DateEnd As Date
            Dim NewVisitorVisits As Integer
            Dim SinglePageVisits As Integer
            Dim AuthenticatedVisits As Integer
            Dim NoCookieVisits As Integer
            Dim AveTimeOnSite As Double
            Dim HitCnt As Integer
            Dim VisitCnt As Integer
            Dim OldestDateAdded As Date
            Dim EmptyVariant As Object
            Dim NeedToClearCache As Boolean
            Dim ArchiveParentID As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            'Dim AddonInstall As New addonInstallClass
            Dim LoopPtr As Integer
            Dim Ptr As Integer
            Dim LocalFile As String
            Dim LocalFilename As String
            Dim Folders() As String
            Dim FolderCnt As Integer
            Dim CollectionGUID As String
            Dim CollectionName As String
            Dim Pos As Integer
            Dim LastChangeDate As String
            Dim SubFolderList As String
            Dim SubFolders() As String
            Dim SubFolder As String
            Dim Cnt As Integer
            Dim LocalGUID As String
            Dim LocalLastChangeDateStr As String
            Dim LocalLastChangeDate As Date
            Dim LibGUID As String
            Dim LibLastChangeDateStr As String
            Dim LibLastChangeDate As Date
            Dim LibListNode As XmlNode
            Dim LocalListNode As XmlNode
            Dim CollectionNode As XmlNode
            Dim LibraryCollections As New XmlDocument
            Dim LocalCollections As New XmlDocument
            Dim Doc As New XmlDocument
            ''Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            Dim SetTimeCheckString As String
            Dim SetTimeCheck As Double
            Dim LogDate As Date
            Dim FolderName As String
            Dim FileList As String
            Dim FileArray() As String
            Dim FileArrayCount As Integer
            Dim FileArrayPointer As Integer
            Dim FileSplit() As String
            Dim FolderList As String
            Dim FolderArray() As String
            Dim FolderArrayCount As Integer
            Dim FolderArrayPointer As Integer
            Dim FolderSplit() As String
            'Dim fs As New fileSystemClass
            Dim VisitArchiveAgeDays As Integer
            Dim NewDay As Boolean
            Dim NewHour As Boolean
            Dim CSPages As Integer
            Dim PageID As Integer
            Dim PageTitle As String
            Dim NoCookiePageViews As Integer
            Dim PageViews As Integer
            Dim AuthenticatedPageViews As Integer
            Dim MobilePageViews As Integer
            Dim BotPageViews As Integer
            '
            Dim LastTimeCheck As Date
            '
            Dim ConfigFilename As String
            Dim Config As String
            Dim ConfigLines() As String
            '
            Dim Line As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            Dim NameValue() As String
            Dim SQLNow As String
            Dim SQL As String
            Dim AveReadTime As Integer

            '
            If BuildVersion < cp.Version Then
                Call HandleClassInternalError(cp.core.serverConfig.appConfig.name, "HouseKeep_PageViewSummary", ignoreInteger, "Can not summarize analytics until this site's data needs been upgraded.")
            Else
                PeriodStart = StartTimeDate
                If PeriodStart < OldestVisitSummaryWeCareAbout Then
                    PeriodStart = OldestVisitSummaryWeCareAbout
                End If
                StartDate = Int(PeriodStart)
                'StartTime = Int((PeriodStart - StartDate) * 24) / 24
                'PeriodStart = StartDate + StartTime
                PeriodStep = CDbl(HourDuration) / 24.0!
                Do While PeriodDatePtr < EndTimeDate
                    '
                    DateNumber = Int(PeriodDatePtr.AddHours(HourDuration / 2).ToOADate)
                    TimeNumber = PeriodDatePtr.TimeOfDay.TotalHours
                    DateStart = PeriodDatePtr.Date
                    DateEnd = PeriodDatePtr.AddHours(HourDuration).Date
                    '
                    VisitCnt = 0
                    HitCnt = 0
                    SumStartTime = 0
                    SumStopTime = 0
                    NewVisitorVisits = 0
                    SinglePageVisits = 0
                    MultiPageVisitCnt = 0
                    MultiPageTimetoLastHitSum = 0
                    AuthenticatedVisits = 0
                    NoCookieVisits = 0
                    AveTimeOnSite = 0
                    PageTitle = ""
                    PageID = 0
                    PageViews = 0
                    AuthenticatedPageViews = 0
                    MobilePageViews = 0
                    BotPageViews = 0
                    NoCookiePageViews = 0
                    '
                    ' Loop through all the pages hit during this period
                    '
                    '
                    ' for now ignore the problem caused by addons like Blogs creating multiple 'pages' within on pageid
                    ' One way to handle this is to expect the addon to set something unquie in he page title
                    ' then use the title to distinguish a page. The problem with this is the current system puts the
                    ' visit number and page number in the name. if we select on district name, they will all be.
                    '
                    SQL = "select distinct recordid,pagetitle from ccviewings h" _
                        & " where (h.recordid<>0)" _
                        & " and(h.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                        & " and (h.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                        & " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))" _
                        & "order by recordid"
                    CSPages = cp.core.db.cs_openCsSql_rev("default", SQL)
                    If Not cp.core.db.cs_ok(CSPages) Then
                        '
                        ' no hits found - add or update a single record for this day so we know it has been calculated
                        '
                        CS = cp.core.db.cs_open("Page View Summary", "(timeduration=" & HourDuration & ")and(DateNumber=" & DateNumber & ")and(TimeNumber=" & TimeNumber & ")and(pageid=" & PageID & ")and(pagetitle=" & cp.core.db.encodeSQLText(PageTitle) & ")")
                        If Not cp.core.db.cs_ok(CS) Then
                            Call cp.core.db.cs_Close(CS)
                            CS = cp.core.db.cs_insertRecord("Page View Summary")
                        End If
                        '
                        If cp.core.db.cs_ok(CS) Then
                            Call cp.core.db.cs_set(CS, "name", HourDuration & " hr summary for " & Date.FromOADate(CDbl(DateNumber)) & " " & TimeNumber & ":00, " & PageTitle)
                            Call cp.core.db.cs_set(CS, "DateNumber", DateNumber)
                            Call cp.core.db.cs_set(CS, "TimeNumber", TimeNumber)
                            Call cp.core.db.cs_set(CS, "TimeDuration", HourDuration)
                            Call cp.core.db.cs_set(CS, "PageViews", PageViews)
                            Call cp.core.db.cs_set(CS, "PageID", PageID)
                            Call cp.core.db.cs_set(CS, "PageTitle", PageTitle)
                            Call cp.core.db.cs_set(CS, "AuthenticatedPageViews", AuthenticatedPageViews)
                            Call cp.core.db.cs_set(CS, "NoCookiePageViews", NoCookiePageViews)
                            If True Then
                                Call cp.core.db.cs_set(CS, "MobilePageViews", MobilePageViews)
                                Call cp.core.db.cs_set(CS, "BotPageViews", BotPageViews)
                            End If
                        End If
                        Call cp.core.db.cs_Close(CS)
                    Else
                        '
                        ' add an entry for each page hit on this day
                        '
                        Do While cp.core.db.cs_ok(CSPages)
                            PageID = cp.core.db.cs_getInteger(CSPages, "recordid")
                            PageTitle = cp.core.db.cs_getText(CSPages, "pagetitle")
                            baseCriteria = "" _
                                & " (h.recordid=" & PageID & ")" _
                                & " " _
                                & " and(h.dateadded>=" & cp.core.db.encodeSQLDate(DateStart) & ")" _
                                & " and(h.dateadded<" & cp.core.db.encodeSQLDate(DateEnd) & ")" _
                                & " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))" _
                                & " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))" _
                                & ""
                            If PageTitle <> "" Then
                                baseCriteria = baseCriteria & "and(h.pagetitle=" & cp.core.db.encodeSQLText(PageTitle) & ")"
                            End If
                            '
                            ' Total Page Views
                            '
                            SQL = "select count(h.id) as cnt" _
                                & " from ccviewings h left join ccvisits v on h.visitid=v.id" _
                                & " where " & baseCriteria _
                                & " and (v.CookieSupport<>0)" _
                                & ""
                            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                            If cp.core.db.cs_ok(CS) Then
                                PageViews = cp.core.db.cs_getInteger(CS, "cnt")
                            End If
                            Call cp.core.db.cs_Close(CS)
                            '
                            ' Authenticated Visits
                            '
                            SQL = "select count(h.id) as cnt" _
                                & " from ccviewings h left join ccvisits v on h.visitid=v.id" _
                                & " where " & baseCriteria _
                                & " and(v.CookieSupport<>0)" _
                                & " and(v.visitAuthenticated<>0)" _
                                & ""
                            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                            If cp.core.db.cs_ok(CS) Then
                                AuthenticatedPageViews = cp.core.db.cs_getInteger(CS, "cnt")
                            End If
                            Call cp.core.db.cs_Close(CS)
                            '
                            ' No Cookie Page Views
                            '
                            SQL = "select count(h.id) as NoCookiePageViews" _
                                & " from ccviewings h left join ccvisits v on h.visitid=v.id" _
                                & " where " & baseCriteria _
                                & " and((v.CookieSupport=0)or(v.CookieSupport is null))" _
                                & ""
                            CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                            If cp.core.db.cs_ok(CS) Then
                                NoCookiePageViews = cp.core.db.cs_getInteger(CS, "NoCookiePageViews")
                            End If
                            Call cp.core.db.cs_Close(CS)
                            '
                            If True Then
                                '
                                ' Mobile Visits
                                '
                                SQL = "select count(h.id) as cnt" _
                                    & " from ccviewings h left join ccvisits v on h.visitid=v.id" _
                                    & " where " & baseCriteria _
                                    & " and(v.CookieSupport<>0)" _
                                    & " and(v.mobile<>0)" _
                                    & ""
                                CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                If cp.core.db.cs_ok(CS) Then
                                    MobilePageViews = cp.core.db.cs_getInteger(CS, "cnt")
                                End If
                                Call cp.core.db.cs_Close(CS)
                                '
                                ' Bot Visits
                                '
                                SQL = "select count(h.id) as cnt" _
                                    & " from ccviewings h left join ccvisits v on h.visitid=v.id" _
                                    & " where " & baseCriteria _
                                    & " and(v.CookieSupport<>0)" _
                                    & " and(v.bot<>0)" _
                                    & ""
                                CS = cp.core.db.cs_openCsSql_rev("default", SQL)
                                If cp.core.db.cs_ok(CS) Then
                                    BotPageViews = cp.core.db.cs_getInteger(CS, "cnt")
                                End If
                                Call cp.core.db.cs_Close(CS)
                            End If
                            '
                            ' Add or update the Visit Summary Record
                            '
                            CS = cp.core.db.cs_open("Page View Summary", "(timeduration=" & HourDuration & ")and(DateNumber=" & DateNumber & ")and(TimeNumber=" & TimeNumber & ")and(pageid=" & PageID & ")and(pagetitle=" & cp.core.db.encodeSQLText(PageTitle) & ")")
                            If Not cp.core.db.cs_ok(CS) Then
                                Call cp.core.db.cs_Close(CS)
                                CS = cp.core.db.cs_insertRecord("Page View Summary")
                            End If
                            '
                            If cp.core.db.cs_ok(CS) Then
                                Dim PageName As String

                                If PageTitle = "" Then
                                    PageName = cp.core.GetRecordName("page content", PageID)
                                    Call cp.core.db.cs_set(CS, "name", HourDuration & " hr summary for " & Date.FromOADate(CDbl(DateNumber)) & " " & TimeNumber & ":00, " & PageName)
                                    Call cp.core.db.cs_set(CS, "PageTitle", PageName)
                                Else
                                    Call cp.core.db.cs_set(CS, "name", HourDuration & " hr summary for " & Date.FromOADate(CDbl(DateNumber)) & " " & TimeNumber & ":00, " & PageTitle)
                                    Call cp.core.db.cs_set(CS, "PageTitle", PageTitle)
                                End If
                                Call cp.core.db.cs_set(CS, "DateNumber", DateNumber)
                                Call cp.core.db.cs_set(CS, "TimeNumber", TimeNumber)
                                Call cp.core.db.cs_set(CS, "TimeDuration", HourDuration)
                                Call cp.core.db.cs_set(CS, "PageViews", PageViews)
                                Call cp.core.db.cs_set(CS, "PageID", PageID)
                                Call cp.core.db.cs_set(CS, "AuthenticatedPageViews", AuthenticatedPageViews)
                                Call cp.core.db.cs_set(CS, "NoCookiePageViews", NoCookiePageViews)
                                If True Then
                                    Call cp.core.db.cs_set(CS, "MobilePageViews", MobilePageViews)
                                    Call cp.core.db.cs_set(CS, "BotPageViews", BotPageViews)
                                End If
                            End If
                            Call cp.core.db.cs_Close(CS)
                            Call cp.core.db.cs_goNext(CSPages)
                        Loop
                    End If
                    Call cp.core.db.cs_Close(CSPages)
                    PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration)
                Loop
            End If
            '
            Exit Sub
ErrorTrap:
            Call HandleClassTrapError(cp.core.serverConfig.appConfig.name, "HouseKeep_PageViewSummary", "Trap", True)
            Err.Clear()
        End Sub
        '
        '====================================================================================================
        Public Sub housekeepAddonFolder()
            Try
                Dim RegisterPathList As String
                Dim RegisterPath As String
                Dim RegisterPaths() As String
                Dim Path As String
                Dim NodeCnt As Integer
                Dim IsActiveFolder As Boolean
                Dim Cmd As String
                Dim CollectionRootPath As String
                Dim Pos As Integer
                Dim FolderList As IO.DirectoryInfo()
                Dim LocalPath As String
                Dim LocalGuid As String
                Dim Doc As New XmlDocument
                Dim CollectionNode As XmlNode
                Dim LocalListNode As XmlNode
                Dim FolderPtr As Integer
                Dim CollectionPath As String
                Dim LastChangeDate As Date
                Dim hint As String
                Dim LocalName As String
                Dim Ptr As Integer
                Dim collectionFileFilename As String
                '
                Call AppendClassLog("Server", "RegisterAddonFolder", "Entering RegisterAddonFolder")
                '
                Dim loadOK As Boolean = True
                Try
                    collectionFileFilename = cp.core.addon.getPrivateFilesAddonPath & "Collections.xml"
                    Call Doc.LoadXml(collectionFileFilename)
                Catch ex As Exception
                    'hint = hint & ",parse error"
                    Call AppendClassLog("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], Error loading Collections.xml file.")
                    loadOK = False
                End Try
                If loadOK Then
                    '
                    Call AppendClassLog("Server", "RegisterAddonFolder", "Collection.xml loaded ok")
                    '
                    If vbLCase(Doc.DocumentElement.Name) <> vbLCase(CollectionListRootNode) Then
                        Call AppendClassLog("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.")
                    Else
                        '
                        Call AppendClassLog("Server", "RegisterAddonFolder", "Collection.xml root name ok")
                        '
                        With Doc.DocumentElement
                            If True Then
                                'If vbLCase(.name) <> "collectionlist" Then
                                '    Call AppendClassLog("Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
                                'Else
                                NodeCnt = 0
                                RegisterPathList = ""
                                For Each LocalListNode In .ChildNodes
                                    '
                                    ' Get the collection path
                                    '
                                    CollectionPath = ""
                                    LocalGuid = ""
                                    LocalName = "no name found"
                                    LocalPath = ""
                                    Select Case vbLCase(LocalListNode.Name)
                                        Case "collection"
                                            LocalGuid = ""
                                            For Each CollectionNode In LocalListNode.ChildNodes
                                                Select Case vbLCase(CollectionNode.Name)
                                                    Case "name"
                                                        '
                                                        LocalName = vbLCase(CollectionNode.InnerText)
                                                    Case "guid"
                                                        '
                                                        LocalGuid = vbLCase(CollectionNode.InnerText)
                                                    Case "path"
                                                        '
                                                        CollectionPath = vbLCase(CollectionNode.InnerText)
                                                    Case "lastchangedate"
                                                        LastChangeDate = EncodeDate(CollectionNode.InnerText)
                                                End Select
                                            Next
                                    End Select
                                    '
                                    Call AppendClassLog("Server", "RegisterAddonFolder", "Node[" & NodeCnt & "], LocalName=[" & LocalName & "], LastChangeDate=[" & LastChangeDate & "], CollectionPath=[" & CollectionPath & "], LocalGuid=[" & LocalGuid & "]")
                                    '
                                    ' Go through all subpaths of the collection path, register the version match, unregister all others
                                    '
                                    'fs = New fileSystemClass
                                    If CollectionPath = "" Then
                                        '
                                        Call AppendClassLog("Server", "RegisterAddonFolder", "no collection path, skipping")
                                        '
                                    Else
                                        CollectionPath = vbLCase(CollectionPath)
                                        CollectionRootPath = CollectionPath
                                        Pos = InStrRev(CollectionRootPath, "\")
                                        If Pos <= 0 Then
                                            '
                                            Call AppendClassLog("Server", "RegisterAddonFolder", "CollectionPath has no '\', skipping")
                                            '
                                        Else
                                            CollectionRootPath = Left(CollectionRootPath, Pos - 1)
                                            Path = cp.core.addon.getPrivateFilesAddonPath() & "\" & CollectionRootPath & "\"
                                            'Path = GetProgramPath & "\addons\" & CollectionRootPath & "\"
                                            'On Error Resume Next
                                            If cp.core.privateFiles.pathExists(Path) Then
                                                FolderList = cp.core.privateFiles.getFolderList(Path)
                                                If Err.Number <> 0 Then
                                                    Err.Clear()
                                                End If
                                            End If
                                            'On Error GoTo ErrorTrap
                                            If FolderList.Length = 0 Then
                                                '
                                                Call AppendClassLog("Server", "RegisterAddonFolder", "no subfolders found in physical path [" & Path & "], skipping")
                                                '
                                            Else
                                                For Each dir As IO.DirectoryInfo In FolderList
                                                    IsActiveFolder = False
                                                    '
                                                    ' register or unregister all files in this folder
                                                    '
                                                    If dir.Name = "" Then
                                                        '
                                                        Call AppendClassLog("Server", "RegisterAddonFolder", "....empty folder [" & dir.Name & "], skipping")
                                                        '
                                                    Else
                                                        '
                                                        Call AppendClassLog("Server", "RegisterAddonFolder", "....Folder [" & dir.Name & "]")
                                                        IsActiveFolder = (CollectionRootPath & "\" & dir.Name = CollectionPath)
                                                        If IsActiveFolder And (FolderPtr <> (FolderList.Count - 1)) Then
                                                            '
                                                            ' This one is active, but not the last
                                                            '
                                                            Call AppendClassLog("Server", "RegisterAddonFolder", "....Active addon is not the most current, this folder is the active folder, but there are more recent folders. This folder will be preserved.")
                                                        End If
                                                        ' 20161005 - no longer need to register activeX
                                                        'FileList = cp.core.app.privateFiles.GetFolderFiles(Path & "\" & dir.Name)
                                                        'For Each file As IO.FileInfo In FileList
                                                        '    If Right(file.Name, 4) = ".dll" Then
                                                        '        If IsActiveFolder Then
                                                        '            '
                                                        '            ' register this file
                                                        '            '
                                                        '            RegisterPathList = RegisterPathList & vbCrLf & Path & dir.Name & "\" & file.Name
                                                        '            '                                                                Cmd = "%comspec% /c regsvr32 """ & RegisterPathList & """ /s"
                                                        '            '                                                                Call AppendClassLog("Server", "RegisterAddonFolder", "....Regsiter DLL [" & Cmd & "]")
                                                        '            '                                                                Call runProcess(cp.core,Cmd, , True)
                                                        '        Else
                                                        '            '
                                                        '            ' unregister this file
                                                        '            '
                                                        '            Cmd = "%comspec% /c regsvr32 /u """ & Path & dir.Name & "\" & file.Name & """ /s"
                                                        '            Call AppendClassLog("Server", "RegisterAddonFolder", "....Unregsiter DLL [" & Cmd & "]")
                                                        '            Call runProcess(cp.core, Cmd, , True)
                                                        '        End If
                                                        '    End If
                                                        'Next
                                                        '
                                                        ' only keep last two non-matching folders and the active folder
                                                        '
                                                        If IsActiveFolder Then
                                                            IsActiveFolder = IsActiveFolder
                                                        Else
                                                            If FolderPtr < (FolderList.Count - 3) Then
                                                                Call AppendClassLog("Server", "RegisterAddonFolder", "....Deleting path because non-active and not one of the newest 2 [" & Path & dir.Name & "]")
                                                                Call cp.core.privateFiles.DeleteFileFolder(Path & dir.Name)
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                                '
                                                ' register files found in the active folder last
                                                '
                                                If RegisterPathList <> "" Then
                                                    RegisterPaths = Split(RegisterPathList, vbCrLf)
                                                    For Ptr = 0 To UBound(RegisterPaths)
                                                        RegisterPath = Trim(RegisterPaths(Ptr))
                                                        If RegisterPath <> "" Then
                                                            Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
                                                            Call AppendClassLog("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
                                                            Call runProcess(cp.core, Cmd, , True)
                                                        End If
                                                    Next
                                                    RegisterPathList = ""
                                                End If
                                            End If
                                        End If
                                    End If
                                    NodeCnt = NodeCnt + 1
                                Next
                                ' 20161005 - no longer need to register activeX
                                ''
                                '' register files found in the active folder last
                                ''
                                'If RegisterPathList <> "" Then
                                '    RegisterPaths = Split(RegisterPathList, vbCrLf)
                                '    For Ptr = 0 To UBound(RegisterPaths)
                                '        RegisterPath = Trim(RegisterPaths(Ptr))
                                '        If RegisterPath <> "" Then
                                '            Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
                                '            Call AppendClassLog("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
                                '            Call runProcess(cp.core, Cmd, , True)
                                '        End If
                                '    Next
                                'End If
                            End If
                        End With
                    End If
                End If
                '
                Call AppendClassLog("Server", "RegisterAddonFolder", "Exiting RegisterAddonFolder")
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex)
            End Try
        End Sub
    End Class
End Namespace
