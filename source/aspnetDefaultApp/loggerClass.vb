
Option Explicit On
Option Strict On

Imports System.IO

Public Class Logger
    Public Shared Sub appendProgramDataLog(ByVal info As String)
        Dim folderName As String = "C:\ProgramData\Contensive"
        Dim fileName As String = "C:\ProgramData\Contensive\" & Now.Year.ToString & Now.Month.ToString().PadLeft(2, "0"c) & Now.Day.ToString().PadLeft(2, "0"c) & ".log"
        Dim dir As DirectoryInfo = New DirectoryInfo(folderName)
        If File.Exists(fileName) Then
            Try
                Dim fs As FileStream = New FileStream(fileName, FileMode.Append, FileAccess.Write)
                Dim sw As StreamWriter = New StreamWriter(fs)
                sw.WriteLine(DateTime.Now.ToString() + " " + info.ToString)
                sw.Close()
                fs.Close()
            Catch Ex As Exception
                '
            End Try
        Else
            Try
                dir = Directory.CreateDirectory(dir.FullName)
                Dim fileStream As FileStream = File.Create(fileName)
                Dim sw As StreamWriter = New StreamWriter(fileStream)
                sw.WriteLine(DateTime.Now.ToString() + info.ToString)
                sw.Close()
                fileStream.Close()
            Catch ex As Exception
                '
            End Try
        End If
    End Sub
End Class
