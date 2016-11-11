
'Imports Contensive.Core
Imports Contensive.Core

Namespace Contensive.serverDebugger

    Public Class Form1
        '
        '
        '
        Private cp As cpclass
        Private server As legacyWorkerClass
        '
        ' form close - stop the server
        '
        Private Sub Form1_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
            Call server.stopServer()
        End Sub
        '
        ' form load - start the server
        '
        Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
            Call main()
        End Sub
        '
        '
        '
        Public Sub main()
            Dim closeOnExit As Boolean = False
            Try
                Dim StartTime As Date
                Dim cmd As String
                '
                'cpCore.AppendLog( "serverDebug, main, enter", "", "trace")
                If server.PrevInstance() Then
                    '
                    ' Block load if this is not the first instance
                    '
                    'cpCore.AppendLog( "serverDebug, main, prevInstance found, exit", "", "trace")
                    Call Console.WriteLine(Now.ToLongTimeString & "Server was not loaded because it is already running in the system.")
                Else
                    '
                    ' Initialize
                    '
                    'cpCore.AppendLog( "serverDebug, main, initialize", "", "trace")
                    StartTime = Now()
                    '
                    ' Perform correct load command
                    '
                    cmd = Command.ToLower.Trim
                    Select Case cmd
                        Case "-buildbase"
                            '
                            ' !!!!! NEED A PLAN FOR THIS
                            ' ----- command line, creates a new base Db
                            '
                            closeOnExit = True
                            'msg = "server-buildbase, Building Base Db, version " & getcodeversion
                            ''Call AppendLog(msg)
                            'Me.TextBox1.Text = msg
                            'Me.Show()
                            'Call Console.WriteLine(msg)
                            'Call server.StartServer(True, True)
                            ''Call server.StartServer(True, False)
                            ''cpCore.AppendLog("server-buildbase, create ContensiveBase application")
                            'Call ctrl.DeleteApplication("ContensiveBase")
                            'status = ctrl.GetApplicationStatus("ContensiveBase")
                            'If status = "" Then
                            '    '
                            '    ' Create new blank Db
                            '    '
                            '    DbPath = getProgramFilesPath() & "\cclib\Samples\Db"
                            '    BlankDbPath = DbPath & "\Blank.mdb"
                            '    BaseDbPath = DbPath & "\Base.mdb"
                            '    Call cpCore.app.publicFiles.CopyFile(BlankDbPath, BaseDbPath)
                            '    '
                            '    ' Build ODBC file DSN
                            '    '
                            '    Content = "[ODBC]" _
                            '        & vbCrLf & "DRIVER=Microsoft Access Driver (*.mdb)" _
                            '        & vbCrLf & "UserCommitSync = Yes" _
                            '        & vbCrLf & "Threads = 3" _
                            '        & vbCrLf & "SafeTransactions = 0" _
                            '        & vbCrLf & "PageTimeout = 5" _
                            '        & vbCrLf & "MaxScanRows = 8" _
                            '        & vbCrLf & "MaxBufferSize = 2048" _
                            '        & vbCrLf & "FIL=MS Access" _
                            '        & vbCrLf & "DriverId = 25" _
                            '        & vbCrLf & "DefaultDir=" & DbPath _
                            '        & vbCrLf & "DBQ=" & BaseDbPath _
                            '        & vbCrLf & ""
                            '    Call cpCore.app.publicFiles.SaveFile("c:\Program files\Common Files\ODBC\Data Sources\ContensiveBase.DSN", Content)
                            '    ctrl.AddApplication("ContensiveBase")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "ALLOWMONITORING", "0")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "AUTOSTART", "0")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "CONNECTIONSTRING", "filedsn=c:\program files\common files\odbc\Data Sources\ContensiveBase.dsn")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "DOMAINNAMELIST", "127.0.0.1")
                            '    ' really do not understand why it was set to " " -- there are detects for <>"" which fail with this
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "PHYSICALFILEPATH", "")
                            '    'Call ctrl.SetApplicationParameter("ContensiveBase", "PHYSICALFILEPATH", " ")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "PHYSICALWWWPATH", "")
                            '    Call ctrl.SetApplicationParameter("ContensiveBase", "ROOTPATH", "/")
                            '    'cpCore.AppendLog("server-buildbase, call cstl.startApplication2")
                            '    Call ctrl.StartApplication2("ContensiveBase", True, "")
                            '    'cpCore.AppendLog("server-buildbase, delete contensivebase application")
                            '    Call ctrl.DeleteApplication("ContensiveBase")
                            'End If
                        Case "-singlethreaded"
                            '
                            ' ----- Load and start in debug mode (start without installing)
                            '
                            closeOnExit = False
                            Me.TextBox1.Text = "Debug Server, single threaded, version " & cp.Version()
                            Me.Refresh()
                            Call Console.WriteLine(Now.ToLongTimeString & "Console Server starting, -singlethreaded")
                            Call server.StartServer(True, True)
                            Call Console.WriteLine(Now.ToLongTimeString & "Console Server started")
                        Case Else
                            '
                            ' ----- Load and start in debug mode (start without installing)
                            '
                            'cpCore.AppendLog( "serverDebug, main, starting...", "", "trace")
                            closeOnExit = False
                            Me.TextBox1.Text = "Debug Mode Server, version " & cp.Version()
                            Me.Refresh()
                            Call Console.WriteLine(Now.ToLongTimeString & "Console Server starting")
                            Call server.StartServer(True, False)
                            Call Console.WriteLine(Now.ToLongTimeString & "Console Server started")
                            'cpCore.AppendLog( "serverDebug, main, started", "", "trace")
                    End Select
                End If
            Catch ex As Exception
                'cpCore.AppendLog( "Unknown error in consolServer, [" & ex.ToString() & "]", "", "")
            Finally
                If closeOnExit Then
                    Call server.stopServer()
                    Me.Close()
                End If
            End Try
        End Sub
        '
        '
        '
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            Call server.Dispose()
            Call cp.Dispose()
            server = Nothing
            cp = Nothing
        End Sub
        '
        '
        '
        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            cp = New CPClass("cluster-mode-not-implemented-yet")
            server = New legacyWorkerClass(cp.core)
        End Sub
    End Class
End Namespace
