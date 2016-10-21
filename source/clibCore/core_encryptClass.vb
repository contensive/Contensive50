
Option Explicit On
Option Strict On

Imports System.Security.Cryptography

Class encryptClass
    '
    ' privateKey
    '
    Private _privateKey As String = ""
    Private cpCore As Contensive.Core.cpCoreClass
    '
    Public Sub New(cpCore As Contensive.Core.cpCoreClass, privateKey As String)
        Me.cpCore = cpCore
        _privateKey = privateKey
    End Sub
    '
    Public Function encrypt(ByVal sourceToEncrypt As String) As String
        Dim returnResult As String = ""
        Try
            Dim Buffer As Byte()
            Dim DES As New TripleDESCryptoServiceProvider()
            Dim hashMD5 As New MD5CryptoServiceProvider()
            Dim DESEncrypt As ICryptoTransform
            '
            If _privateKey = "" Then
                '
            Else
                ' Compute the MD5 hash.
                DES.Key = hashMD5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(_privateKey))
                ' Set the cipher mode.
                DES.Mode = CipherMode.ECB
                ' Create the encryptor.
                DESEncrypt = DES.CreateEncryptor()
                ' Get a byte array of the string.
                Buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sourceToEncrypt)
                ' Transform and return the string.
                Buffer = DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length)
                returnResult = Convert.ToBase64String(Buffer)
            End If
        Catch ex As Exception
            cpCore.handleException(ex)
        End Try
        Return returnResult
    End Function

    Public Function decrypt(ByVal sourceToDecrypt As String) As String
        Dim returnResult As String = ""
        Try
            Dim buffer() As Byte
            Dim DES As New TripleDESCryptoServiceProvider()
            Dim hashMD5 As New MD5CryptoServiceProvider()
            '
            If String.IsNullOrEmpty(sourceToDecrypt) Then
                '
            Else
                ' Compute the MD5 hash.
                buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(_privateKey)
                DES.Key = hashMD5.ComputeHash(buffer)
                ' Set the cipher mode.
                DES.Mode = CipherMode.ECB
                ' Create the decryptor.
                Dim DESDecrypt As ICryptoTransform = DES.CreateDecryptor()
                buffer = Convert.FromBase64String(sourceToDecrypt)
                Try
                    ' Transform and return the string.
                    returnResult = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length))
                Catch ex As Exception
                    cpCore.handleException(ex)
                End Try
            End If
        Catch ex As Exception
            cpCore.handleException(ex)
        End Try
        Return returnResult
    End Function
End Class
