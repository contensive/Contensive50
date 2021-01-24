Imports System.IO

Namespace DefaultSite
    Public Class WindowsTempFileController
        Public Shared Function CreateTmpFile() As String
            Dim fileName As String = String.Empty

            Try
                fileName = Path.GetTempFileName()
                Dim fileInfo As FileInfo = New FileInfo(fileName)
                fileInfo.Attributes = FileAttributes.Temporary
                Console.WriteLine("TEMP file created at: " & fileName)
            Catch ex As Exception
                Console.WriteLine("Unable to create TEMP file or set its attributes: " & ex.Message)
            End Try

            Return fileName
        End Function

        Public Shared Sub updateTmpFile(ByVal tmpFile As String, ByVal content As String)
            Try
                Dim streamWriter As StreamWriter = File.AppendText(tmpFile)
                streamWriter.Write(content)
                streamWriter.Flush()
                streamWriter.Close()
                Console.WriteLine("TEMP file updated.")
            Catch ex As Exception
                Console.WriteLine("Error writing to TEMP file: " & ex.Message)
            End Try
        End Sub

        Public Shared Sub readTmpFile(ByVal tmpFile As String)
            Try
                Dim myReader As StreamReader = File.OpenText(tmpFile)
                Console.WriteLine("TEMP file contents: " & myReader.ReadToEnd())
                myReader.Close()
            Catch ex As Exception
                Console.WriteLine("Error reading TEMP file: " & ex.Message)
            End Try
        End Sub

        Public Shared Sub deleteTmpFile(ByVal tmpFile As String)
            Try

                If File.Exists(tmpFile) Then
                    File.Delete(tmpFile)
                    Console.WriteLine("TEMP file deleted.")
                End If

            Catch ex As Exception
                Console.WriteLine("Error deleteing TEMP file: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace
