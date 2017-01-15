
Option Explicit On
Option Strict On

Imports System.Text
Imports System.Security.Cryptography

Namespace Contensive.Core
    Public Class coreSecurityClass
        '
        ' privateKey
        '
        Private _privateKey As String = ""
        Private cpCore As Contensive.Core.coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="privateKey"></param>
        Public Sub New(cpCore As Contensive.Core.coreClass, privateKey As String)
            Me.cpCore = cpCore
            _privateKey = privateKey
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' return an encrypted string. This is a one way so use it passwords, etc.
        ''' </summary>
        ''' <param name="password"></param>
        ''' <returns></returns>
        Public Function oneWayEncrypt(ByVal password As String) As String
            Dim returnResult As String = ""
            Try
                returnResult = hashEncode.ComputeHash(password, "SHA512", Nothing)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return if an encrypted string matches an unencrypted string.
        ''' </summary>
        ''' <param name="sourceToTest"></param>
        ''' <returns></returns>
        Public Function oneWayVerify(ByVal sourceToTest As String, encryptedTaken As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                returnResult = hashEncode.VerifyHash(sourceToTest, "SHA512", encryptedTaken)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return an encrypted string. This is a two way so use it for little sister security, not foreign government security
        ''' </summary>
        ''' <param name="sourceToEncrypt"></param>
        ''' <returns></returns>
        Public Function twoWayEncrypt(ByVal sourceToEncrypt As String) As String
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
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return an decrypted string. This is a two way so use it for little sister security, not foreign government security
        ''' </summary>
        ''' <param name="sourceToDecrypt"></param>
        ''' <returns></returns>
        Public Function twoWayDecrypt(ByVal sourceToDecrypt As String) As String
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
                        cpCore.handleExceptionAndRethrow(ex)
                    End Try
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        ''' <summary>
        ''' This class generates and compares hashes using MD5, SHA1, SHA256, SHA384, 
        ''' and SHA512 hashing algorithms. Before computing a hash, it appends a
        ''' randomly generated salt to the plain text, and stores this salt appended
        ''' to the result. To verify another plain text value against the given hash,
        ''' this class will retrieve the salt value from the hash string and use it
        ''' when computing a new hash of the plain text. Appending a salt value to
        ''' the hash may not be the most efficient approach, so when using hashes in
        ''' a real-life application, you may choose to store them separately. You may
        ''' also opt to keep results as byte arrays instead of converting them into
        ''' base64-encoded strings.
        ''' </summary>
        Private Class hashEncode
            '
            '==========================================================================================
            ''' <summary>
            ''' Generates a hash for the given plain text value and returns a
            ''' base64-encoded result. Before the hash is computed, a random salt
            ''' is generated and appended to the plain text. This salt is stored at
            ''' the end of the hash value, so it can be used later for hash
            ''' verification.
            ''' </summary>
            ''' <param name="plainText">
            ''' Plaintext value to be hashed. The function does not check whether
            ''' this parameter is null.
            ''' </param>
            ''' <param name="hashAlgorithm">
            ''' Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
            ''' "SHA256", "SHA384", and "SHA512" (if any other value is specified
            ''' MD5 hashing algorithm will be used). This value is case-insensitive.
            ''' </param>
            ''' <param name="saltBytes">
            ''' Salt bytes. This parameter can be null, in which case a random salt
            ''' value will be generated.
            ''' </param>
            ''' <returns>
            ''' Hash value formatted as a base64-encoded string.
            ''' </returns>
            Public Shared Function ComputeHash(ByVal plainText As String,
                                               ByVal hashAlgorithm As String,
                                               ByVal saltBytes() As Byte) _
                                   As String

                ' If salt is not specified, generate it on the fly.
                If (saltBytes Is Nothing) Then

                    ' Define min and max salt sizes.
                    Dim minSaltSize As Integer
                    Dim maxSaltSize As Integer

                    minSaltSize = 4
                    maxSaltSize = 8

                    ' Generate a random number for the size of the salt.
                    Dim random As Random
                    random = New Random()

                    Dim saltSize As Integer
                    saltSize = random.Next(minSaltSize, maxSaltSize)

                    ' Allocate a byte array, which will hold the salt.
                    saltBytes = New Byte(saltSize - 1) {}

                    ' Initialize a random number generator.
                    Dim rng As RNGCryptoServiceProvider
                    rng = New RNGCryptoServiceProvider()

                    ' Fill the salt with cryptographically strong byte values.
                    rng.GetNonZeroBytes(saltBytes)
                End If

                ' Convert plain text into a byte array.
                Dim plainTextBytes As Byte()
                plainTextBytes = Encoding.UTF8.GetBytes(plainText)

                ' Allocate array, which will hold plain text and salt.
                Dim plainTextWithSaltBytes() As Byte =
                    New Byte(plainTextBytes.Length + saltBytes.Length - 1) {}

                ' Copy plain text bytes into resulting array.
                Dim I As Integer
                For I = 0 To plainTextBytes.Length - 1
                    plainTextWithSaltBytes(I) = plainTextBytes(I)
                Next I

                ' Append salt bytes to the resulting array.
                For I = 0 To saltBytes.Length - 1
                    plainTextWithSaltBytes(plainTextBytes.Length + I) = saltBytes(I)
                Next I

                ' Because we support multiple hashing algorithms, we must define
                ' hash object as a common (abstract) base class. We will specify the
                ' actual hashing algorithm class later during object creation.
                Dim hash As HashAlgorithm

                ' Make sure hashing algorithm name is specified.
                If (hashAlgorithm Is Nothing) Then
                    hashAlgorithm = ""
                End If

                ' Initialize appropriate hashing algorithm class.
                Select Case hashAlgorithm.ToUpper()

                    Case "SHA1"
                        hash = New SHA1Managed()

                    Case "SHA256"
                        hash = New SHA256Managed()

                    Case "SHA384"
                        hash = New SHA384Managed()

                    Case "SHA512"
                        hash = New SHA512Managed()

                    Case Else
                        hash = New MD5CryptoServiceProvider()

                End Select

                ' Compute hash value of our plain text with appended salt.
                Dim hashBytes As Byte()
                hashBytes = hash.ComputeHash(plainTextWithSaltBytes)

                ' Create array which will hold hash and original salt bytes.
                Dim hashWithSaltBytes() As Byte =
                                           New Byte(hashBytes.Length +
                                                    saltBytes.Length - 1) {}

                ' Copy hash bytes into resulting array.
                For I = 0 To hashBytes.Length - 1
                    hashWithSaltBytes(I) = hashBytes(I)
                Next I

                ' Append salt bytes to the result.
                For I = 0 To saltBytes.Length - 1
                    hashWithSaltBytes(hashBytes.Length + I) = saltBytes(I)
                Next I

                ' Convert result into a base64-encoded string.
                Dim hashValue As String
                hashValue = Convert.ToBase64String(hashWithSaltBytes)

                ' Return the result.
                ComputeHash = hashValue
            End Function

            '
            '====================================================================================================
            ''' <summary>
            ''' Compares a hash of the specified plain text value to a given hash
            ''' value. Plain text is hashed with the same salt value as the original
            ''' hash.
            ''' </summary>
            ''' <param name="plainText">
            ''' Plain text to be verified against the specified hash. The function
            ''' does not check whether this parameter is null.
            ''' </param>
            ''' <param name="hashAlgorithm">
            ''' Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
            ''' "SHA256", "SHA384", and "SHA512" (if any other value is specified
            ''' MD5 hashing algorithm will be used). This value is case-insensitive.
            ''' </param>
            ''' < name="hashValue">
            ''' Base64-encoded hash value produced by ComputeHash function. This value
            ''' includes the original salt appended to it.
            ''' </param>
            ''' <returns>
            ''' If computed hash mathes the specified hash the function the return
            ''' value is true; otherwise, the function returns false.
            ''' </returns>
            Public Shared Function VerifyHash(ByVal plainText As String,
                                              ByVal hashAlgorithm As String,
                                              ByVal hashValue As String) _
                                   As Boolean

                ' Convert base64-encoded hash value into a byte array.
                Dim hashWithSaltBytes As Byte()
                hashWithSaltBytes = Convert.FromBase64String(hashValue)

                ' We must know size of hash (without salt).
                Dim hashSizeInBits As Integer
                Dim hashSizeInBytes As Integer

                ' Make sure that hashing algorithm name is specified.
                If (hashAlgorithm Is Nothing) Then
                    hashAlgorithm = ""
                End If

                ' Size of hash is based on the specified algorithm.
                Select Case hashAlgorithm.ToUpper()

                    Case "SHA1"
                        hashSizeInBits = 160

                    Case "SHA256"
                        hashSizeInBits = 256

                    Case "SHA384"
                        hashSizeInBits = 384

                    Case "SHA512"
                        hashSizeInBits = 512

                    Case Else ' Must be MD5
                        hashSizeInBits = 128

                End Select

                ' Convert size of hash from bits to bytes.
                hashSizeInBytes = CInt(hashSizeInBits / 8)

                ' Make sure that the specified hash value is long enough.
                If (hashWithSaltBytes.Length < hashSizeInBytes) Then
                    VerifyHash = False
                End If

                ' Allocate array to hold original salt bytes retrieved from hash.
                Dim saltBytes() As Byte = New Byte(hashWithSaltBytes.Length -
                                                   hashSizeInBytes - 1) {}

                ' Copy salt from the end of the hash to the new array.
                Dim I As Integer
                For I = 0 To saltBytes.Length - 1
                    saltBytes(I) = hashWithSaltBytes(hashSizeInBytes + I)
                Next I

                ' Compute a new hash string.
                Dim expectedHashString As String
                expectedHashString = ComputeHash(plainText, hashAlgorithm, saltBytes)

                ' If the computed hash matches the specified hash,
                ' the plain text value must be correct.
                VerifyHash = (hashValue = expectedHashString)
            End Function
        End Class
        '
        '====================================================================================================
        '''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Private Sub sampleUse()

            Dim password As String    ' original password
            Dim wrongPassword As String    ' wrong password

            password = "myP@5sw0rd"
            wrongPassword = "password"

            Dim passwordHashMD5 As String
            Dim passwordHashSha1 As String
            Dim passwordHashSha256 As String
            Dim passwordHashSha384 As String
            Dim passwordHashSha512 As String

            passwordHashMD5 =
                   hashEncode.ComputeHash(password, "MD5", Nothing)
            passwordHashSha1 =
                   hashEncode.ComputeHash(password, "SHA1", Nothing)
            passwordHashSha256 =
                   hashEncode.ComputeHash(password, "SHA256", Nothing)
            passwordHashSha384 =
                   hashEncode.ComputeHash(password, "SHA384", Nothing)
            passwordHashSha512 =
                   hashEncode.ComputeHash(password, "SHA512", Nothing)

            Console.WriteLine("COMPUTING HASH VALUES")
            Console.WriteLine("")
            Console.WriteLine("MD5   : {0}", passwordHashMD5)
            Console.WriteLine("SHA1  : {0}", passwordHashSha1)
            Console.WriteLine("SHA256: {0}", passwordHashSha256)
            Console.WriteLine("SHA384: {0}", passwordHashSha384)
            Console.WriteLine("SHA512: {0}", passwordHashSha512)
            Console.WriteLine("")

            Console.WriteLine("COMPARING PASSWORD HASHES")
            Console.WriteLine("")
            Console.WriteLine("MD5    (good): {0}",
                                hashEncode.VerifyHash(
                                password, "MD5",
                                passwordHashMD5).ToString())
            Console.WriteLine("MD5    (bad) : {0}",
                                hashEncode.VerifyHash(
                                wrongPassword, "MD5",
                                passwordHashMD5).ToString())
            Console.WriteLine("SHA1   (good): {0}",
                                hashEncode.VerifyHash(
                                password, "SHA1",
                                passwordHashSha1).ToString())
            Console.WriteLine("SHA1   (bad) : {0}",
                                hashEncode.VerifyHash(
                                wrongPassword, "SHA1",
                                passwordHashSha1).ToString())
            Console.WriteLine("SHA256 (good): {0}",
                                hashEncode.VerifyHash(
                                password, "SHA256",
                                passwordHashSha256).ToString())
            Console.WriteLine("SHA256 (bad) : {0}",
                                hashEncode.VerifyHash(
                                wrongPassword, "SHA256",
                                passwordHashSha256).ToString())
            Console.WriteLine("SHA384 (good): {0}",
                                hashEncode.VerifyHash(
                                password, "SHA384",
                                passwordHashSha384).ToString())
            Console.WriteLine("SHA384 (bad) : {0}",
                                hashEncode.VerifyHash(
                                wrongPassword, "SHA384",
                                passwordHashSha384).ToString())
            Console.WriteLine("SHA512 (good): {0}",
                                hashEncode.VerifyHash(
                                password, "SHA512",
                                passwordHashSha512).ToString())
            Console.WriteLine("SHA512 (bad) : {0}",
                                hashEncode.VerifyHash(
                                wrongPassword, "SHA512",
                                passwordHashSha512).ToString())
        End Sub
        '
        '========================================================================
        '
        Public Function encodeToken(ByVal keyInteger As Integer, ByVal keyDate As Date) As String
            Dim returnToken As String = ""
            Try
                Dim sourceText As String = keyInteger.ToString & vbTab & keyDate.ToString
                returnToken = twoWayEncrypt(sourceText)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "EncodeToken failure. Returning blank result for keyInteger [" & keyInteger & "], keyDate [" & keyDate & "]")
                returnToken = ""
            End Try
            Return returnToken
        End Function
        '
        '========================================================================
        '   Decode a value from an encodestring value
        '       result is 0 if there was a decode error
        '========================================================================
        '
        Public Sub decodeToken(ByVal token As String, ByRef returnNumber As Integer, ByRef returnDate As Date)
            Try
                Dim decodedString As String = ""
                Dim parts As String()
                '
                decodedString = twoWayDecrypt(token)
                parts = decodedString.Split(CChar(vbTab))
                If parts.Length = 2 Then '
                    returnNumber = EncodeInteger(parts(0))
                    returnDate = EncodeDate(parts(1))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "DecodeToken failure. Returning blank result for token [" & token & "]")
                returnNumber = 0
                returnDate = Date.MinValue
            End Try
        End Sub
    End Class
End Namespace
