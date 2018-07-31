
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
using System.Text;
using System.Security.Cryptography;
//
namespace Contensive.Processor.Controllers {
    public class SecurityController {
        //
        //====================================================================================================
        /// <summary>
        /// return an encrypted string. This is a one way so use it passwords, etc.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static  string oneWayEncrypt(CoreController core, string password) {
            string returnResult = "";
            try {
                returnResult = hashEncode.ComputeHash(password, "SHA512", null);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return if an encrypted string matches an unencrypted string.
        /// </summary>
        /// <param name="sourceToTest"></param>
        /// <returns></returns>
        public static bool oneWayVerify(CoreController core, string sourceToTest, string encryptedTaken) {
            bool returnResult = false;
            try {
                returnResult = hashEncode.VerifyHash(sourceToTest, "SHA512", encryptedTaken);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return an encrypted string. This is a two way so use it for little sister security, not foreign government security
        /// </summary>
        /// <param name="sourceToEncrypt"></param>
        /// <returns></returns>
        public static  string twoWayEncrypt(CoreController core, string sourceToEncrypt) {
            string returnResult = "";
            try {
                byte[] Buffer = null;
                TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();
                ICryptoTransform DESEncrypt = null;
                //
                if (string.IsNullOrEmpty(core.appConfig.privateKey)) {
                    //
                } else {
                    // Compute the MD5 hash.
                    DES.Key = hashMD5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(core.appConfig.privateKey));
                    // Set the cipher mode.
                    DES.Mode = CipherMode.ECB;
                    // Create the encryptor.
                    DESEncrypt = DES.CreateEncryptor();
                    // Get a byte array of the string.
                    Buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sourceToEncrypt);
                    // Transform and return the string.
                    Buffer = DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length);
                    returnResult = Convert.ToBase64String(Buffer);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return an decrypted string. blank or non-base64 strings return an empty string. Exception thrown if decryption error. This is a two way so use it for little sister security, not foreign government security
        /// </summary>
        /// <param name="sourceToDecrypt"></param>
        /// <returns></returns>
        public static  string twoWayDecrypt(CoreController core, string sourceToDecrypt) {
            string returnResult = "";
            try {
                byte[] buffer = null;
                //
                if (string.IsNullOrEmpty(sourceToDecrypt)) {
                    //
                    // -- source blank, decrypt to blank
                } else if (!sourceToDecrypt.IsBase64String()) {
                    //
                    // -- source invalid, decrypt to blank
                } else {
                    // Compute the MD5 hash.
                    buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(core.appConfig.privateKey);
                    MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider();
                    TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
                    DES.Key = hashMD5.ComputeHash(buffer);
                    // Set the cipher mode.
                    DES.Mode = CipherMode.ECB;
                    // Create the decryptor.
                    ICryptoTransform DESDecrypt = DES.CreateDecryptor();
                    buffer = Convert.FromBase64String(sourceToDecrypt);
                    try {
                        // Transform and return the string.
                        returnResult = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                    } catch (Exception ex) {
                        logController.handleError( core,ex);
                        throw;
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        /// <summary>
        /// This class generates and compares hashes using MD5, SHA1, SHA256, SHA384, 
        /// and SHA512 hashing algorithms. Before computing a hash, it appends a
        /// randomly generated salt to the plain text, and stores this salt appended
        /// to the result. To verify another plain text value against the given hash,
        /// this class will retrieve the salt value from the hash string and use it
        /// when computing a new hash of the plain text. Appending a salt value to
        /// the hash may not be the most efficient approach, so when using hashes in
        /// a real-life application, you may choose to store them separately. You may
        /// also opt to keep results as byte arrays instead of converting them into
        /// base64-encoded strings.
        /// </summary>
        private class hashEncode {
            //
            //==========================================================================================
            /// <summary>
            /// Generates a hash for the given plain text value and returns a
            /// base64-encoded result. Before the hash is computed, a random salt
            /// is generated and appended to the plain text. This salt is stored at
            /// the end of the hash value, so it can be used later for hash
            /// verification.
            /// </summary>
            /// <param name="plainText">
            /// Plaintext value to be hashed. The function does not check whether
            /// this parameter is null.
            /// </param>
            /// <param name="hashAlgorithm">
            /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
            /// "SHA256", "SHA384", and "SHA512" (if any other value is specified
            /// MD5 hashing algorithm will be used). This value is case-insensitive.
            /// </param>
            /// <param name="saltBytes">
            /// Salt bytes. This parameter can be null, in which case a random salt
            /// value will be generated.
            /// </param>
            /// <returns>
            /// Hash value formatted as a base64-encoded string.
            /// </returns>
            public static string ComputeHash(string plainText, string hashAlgorithm, byte[] saltBytes) {

                // If salt is not specified, generate it on the fly.
                if (saltBytes == null) {

                    // Define min and max salt sizes.
                    int minSaltSize = 0;
                    int maxSaltSize = 0;

                    minSaltSize = 4;
                    maxSaltSize = 8;

                    // Generate a random number for the size of the salt.
                    Random random = null;
                    random = new Random();

                    int saltSize = random.Next(minSaltSize, maxSaltSize);

                    // Allocate a byte array, which will hold the salt.
                    saltBytes = new byte[saltSize];

                    // Initialize a random number generator.
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                    // Fill the salt with cryptographically strong byte values.
                    rng.GetNonZeroBytes(saltBytes);
                }

                // Convert plain text into a byte array.
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                // Allocate array, which will hold plain text and salt.
                byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

                // Copy plain text bytes into resulting array.
                int I = 0;
                for (I = 0; I < plainTextBytes.Length; I++) {
                    plainTextWithSaltBytes[I] = plainTextBytes[I];
                }

                // Append salt bytes to the resulting array.
                for (I = 0; I < saltBytes.Length; I++) {
                    plainTextWithSaltBytes[plainTextBytes.Length + I] = saltBytes[I];
                }

                // Because we support multiple hashing algorithms, we must define
                // hash object as a common (abstract) base class. We will specify the
                // actual hashing algorithm class later during object creation.
                HashAlgorithm hash = null;

                // Make sure hashing algorithm name is specified.
                if (hashAlgorithm == null) {
                    hashAlgorithm = "";
                }

                // Initialize appropriate hashing algorithm class.
                switch (hashAlgorithm.ToUpper()) {

                    case "SHA1":
                        hash = new SHA1Managed();

                        break;
                    case "SHA256":
                        hash = new SHA256Managed();

                        break;
                    case "SHA384":
                        hash = new SHA384Managed();

                        break;
                    case "SHA512":
                        hash = new SHA512Managed();

                        break;
                    default:
                        hash = new MD5CryptoServiceProvider();

                        break;
                }

                // Compute hash value of our plain text with appended salt.
                byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

                // Create array which will hold hash and original salt bytes.
                byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

                // Copy hash bytes into resulting array.
                for (I = 0; I < hashBytes.Length; I++) {
                    hashWithSaltBytes[I] = hashBytes[I];
                }

                // Append salt bytes to the result.
                for (I = 0; I < saltBytes.Length; I++) {
                    hashWithSaltBytes[hashBytes.Length + I] = saltBytes[I];
                }

                // Convert result into a base64-encoded string.
                string hashValue = Convert.ToBase64String(hashWithSaltBytes);

                // Return the result.
                return hashValue;
            }

            //
            //====================================================================================================
            /// <summary>
            /// Compares a hash of the specified plain text value to a given hash
            /// value. Plain text is hashed with the same salt value as the original
            /// hash.
            /// </summary>
            /// <param name="plainText">
            /// Plain text to be verified against the specified hash. The function
            /// does not check whether this parameter is null.
            /// </param>
            /// <param name="hashAlgorithm">
            /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
            /// "SHA256", "SHA384", and "SHA512" (if any other value is specified
            /// MD5 hashing algorithm will be used). This value is case-insensitive.
            /// </param>
            /// < name="hashValue">
            /// Base64-encoded hash value produced by ComputeHash function. This value
            /// includes the original salt appended to it.
            /// </param>
            /// <returns>
            /// If computed hash mathes the specified hash the function the return
            /// value is true; otherwise, the function returns false.
            /// </returns>
            public static bool VerifyHash(string plainText, string hashAlgorithm, string hashValue) {
                // Convert base64-encoded hash value into a byte array.
                byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);
                // We must know size of hash (without salt).
                int hashSizeInBits = 0;
                int hashSizeInBytes = 0;
                // Make sure that hashing algorithm name is specified.
                if (hashAlgorithm == null) {
                    hashAlgorithm = "";
                }
                // Size of hash is based on the specified algorithm.
                switch (hashAlgorithm.ToUpper()) {
                    case "SHA1":
                        hashSizeInBits = 160;
                        break;
                    case "SHA256":
                        hashSizeInBits = 256;
                        break;
                    case "SHA384":
                        hashSizeInBits = 384;
                        break;
                    case "SHA512":
                        hashSizeInBits = 512;
                        break;
                    default: // Must be MD5
                        hashSizeInBits = 128;
                        break;
                }
                // Convert size of hash from bits to bytes.
                hashSizeInBytes = encodeInteger(hashSizeInBits / 8.0);
                // Make sure that the specified hash value is long enough.
                if (hashWithSaltBytes.Length < hashSizeInBytes) {
                }
                // Allocate array to hold original salt bytes retrieved from hash.
                byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];
                // Copy salt from the end of the hash to the new array.
                int I = 0;
                for (I = 0; I < saltBytes.Length; I++) {
                    saltBytes[I] = hashWithSaltBytes[hashSizeInBytes + I];
                }
                // Compute a new hash string.
                string expectedHashString = ComputeHash(plainText, hashAlgorithm, saltBytes);
                return (hashValue == expectedHashString);
            }
        }
        //
        //====================================================================================================
        // <summary>
        /// The main entry point for the application.
        /// </summary>
        private void sampleUse() {

            string password = null; // original password
            string wrongPassword = null; // wrong password

            password = "myP@5sw0rd";
            wrongPassword = "password";

            string passwordHashMD5 = null;
            string passwordHashSha1 = null;
            string passwordHashSha256 = null;
            string passwordHashSha384 = null;
            string passwordHashSha512 = null;

            passwordHashMD5 = hashEncode.ComputeHash(password, "MD5", null);
            passwordHashSha1 = hashEncode.ComputeHash(password, "SHA1", null);
            passwordHashSha256 = hashEncode.ComputeHash(password, "SHA256", null);
            passwordHashSha384 = hashEncode.ComputeHash(password, "SHA384", null);
            passwordHashSha512 = hashEncode.ComputeHash(password, "SHA512", null);

            Console.WriteLine("COMPUTING HASH VALUES");
            Console.WriteLine("");
            Console.WriteLine("MD5   : {0}", passwordHashMD5);
            Console.WriteLine("SHA1  : {0}", passwordHashSha1);
            Console.WriteLine("SHA256: {0}", passwordHashSha256);
            Console.WriteLine("SHA384: {0}", passwordHashSha384);
            Console.WriteLine("SHA512: {0}", passwordHashSha512);
            Console.WriteLine("");

            Console.WriteLine("COMPARING PASSWORD HASHES");
            Console.WriteLine("");
            Console.WriteLine("MD5    (good): {0}", hashEncode.VerifyHash(password, "MD5", passwordHashMD5).ToString());
            Console.WriteLine("MD5    (bad) : {0}", hashEncode.VerifyHash(wrongPassword, "MD5", passwordHashMD5).ToString());
            Console.WriteLine("SHA1   (good): {0}", hashEncode.VerifyHash(password, "SHA1", passwordHashSha1).ToString());
            Console.WriteLine("SHA1   (bad) : {0}", hashEncode.VerifyHash(wrongPassword, "SHA1", passwordHashSha1).ToString());
            Console.WriteLine("SHA256 (good): {0}", hashEncode.VerifyHash(password, "SHA256", passwordHashSha256).ToString());
            Console.WriteLine("SHA256 (bad) : {0}", hashEncode.VerifyHash(wrongPassword, "SHA256", passwordHashSha256).ToString());
            Console.WriteLine("SHA384 (good): {0}", hashEncode.VerifyHash(password, "SHA384", passwordHashSha384).ToString());
            Console.WriteLine("SHA384 (bad) : {0}", hashEncode.VerifyHash(wrongPassword, "SHA384", passwordHashSha384).ToString());
            Console.WriteLine("SHA512 (good): {0}", hashEncode.VerifyHash(password, "SHA512", passwordHashSha512).ToString());
            Console.WriteLine("SHA512 (bad) : {0}", hashEncode.VerifyHash(wrongPassword, "SHA512", passwordHashSha512).ToString());
        }
        //
        //========================================================================
        //
        public static string encodeToken(CoreController core, int keyInteger, DateTime keyDate) {
            string returnToken = "";
            try {
                string sourceText = keyInteger.ToString() + "\t" + keyDate.ToString();
                returnToken = twoWayEncrypt(core,sourceText);
            } catch (Exception ex) {
                logController.handleError( core,ex, "EncodeToken failure. Returning blank result for keyInteger [" + keyInteger + "], keyDate [" + keyDate + "]");
                returnToken = "";
            }
            return returnToken;
        }
        //
        //========================================================================
        //   Decode a value from an encodestring value
        //       result is 0 if there was a decode error
        //========================================================================
        //
        public static void decodeToken(CoreController core, string token, ref int returnNumber, ref DateTime returnDate) {
            try {
                string decodedString = "";
                string[] parts = null;
                //
                decodedString = twoWayDecrypt(core, token);
                parts = decodedString.Split(Convert.ToChar("\t"));
                if (parts.Length == 2) {
                    returnNumber = genericController.encodeInteger(parts[0]);
                    returnDate = genericController.encodeDate(parts[1]);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex, "DecodeToken failure. Returning blank result for token [" + token + "]");
                returnNumber = 0;
                returnDate = DateTime.MinValue;
            }
        }
    }
}
