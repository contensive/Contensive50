
Option Explicit On
Option Strict On

Imports System.IO

Public Class Logger
    Private Shared Sub Info(ByVal info As Object)

        'Gets folder & file information of the log file
        Dim folderName As String = "c:\inetpub\aspxLogs" 'ConfigurationManager.AppSettings("").ToString()
        Dim fileName As String = "c:\inetpub\aspxLogs\aspx.log" ' ConfigurationManager.AppSettings("").ToString()
        Dim dir As DirectoryInfo = New DirectoryInfo(folderName)

        'Check for existence of logger file
        If File.Exists(fileName) Then
            Try
                Dim fs As FileStream = New FileStream(fileName, FileMode.Append, FileAccess.Write)
                Dim sw As StreamWriter = New StreamWriter(fs)
                sw.WriteLine(DateTime.Now.ToString() + " " + info.ToString)
                sw.Close()
                fs.Close()
            Catch dirEx As DirectoryNotFoundException
                LogInfo(dirEx)
            Catch ex As FileNotFoundException
                LogInfo(ex)
            Catch Ex As Exception
                LogInfo(Ex)
            End Try
        Else
            'If file doesn't exist create one
            Try
                dir = Directory.CreateDirectory(dir.FullName)
                Dim fileStream As FileStream = File.Create(fileName)
                Dim sw As StreamWriter = New StreamWriter(fileStream)
                sw.WriteLine(DateTime.Now.ToString() + info.ToString)
                sw.Close()
                fileStream.Close()
            Catch fileEx As FileNotFoundException
                LogInfo(fileEx)
            Catch dirEx As DirectoryNotFoundException
                LogInfo(dirEx)
            Catch ex As Exception
                LogInfo(ex)
            End Try
        End If
    End Sub
    Public Shared Sub LogInfo(ByVal ex As Exception)
        Try

            'Writes error information to the log file including name of the file, line number & error message description
            Dim trace As Diagnostics.StackTrace = New Diagnostics.StackTrace(ex, True)
            Dim fileNames As String = trace.GetFrame((trace.FrameCount - 1)).GetFileName()
            Dim lineNumber As Int32 = trace.GetFrame((trace.FrameCount - 1)).GetFileLineNumber()

            Info("Error In" + fileNames + "Line Number" + lineNumber.ToString() + "Error Message" + ex.Message)
        Catch genEx As Exception
            Info(ex.Message)
        End Try

    End Sub

    Public Shared Sub LogInfo(ByVal message As String)
        Try

            'Write general message to the log file
            Info("Message" + message)
        Catch genEx As Exception
            Info(genEx.Message)
        End Try

    End Sub
End Class
