
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
// todo -- this should use the filesystem objects, not system.io
using System.IO;
using System.Net;
//
namespace Contensive.Processor.Controllers {
    //
    public class httpRequestController {
        //
        private webClientExt http;
        private System.Net.WebHeaderCollection privateRequestHeaders;
        private string privateRequestPassword;
        private string privateRequestUsername;
        private string privateRequestUserAgent;
        private string privateRequestCookie;
        private int privateRequestTimeoutMsec;
        //
        private string privateResponseFilename;
        private string privateResponseProtocol = "HTTP/1.1"; // had to fake bc webClient removes first line of header
        private string privateResponseStatusDescription;
        private int privateResponseStatusCode;
        //Private privateResponseStatus As System.Net.HttpStatusCode = New System.Net.HttpStatusCode
        private System.Net.WebHeaderCollection privateResponseHeaders = new System.Net.WebHeaderCollection();
        private int privateResponseLength = 0;
        //
        private string privateSocketResponse = "";
        //
        //======================================================================================
        // constructor
        //======================================================================================
        //
        public httpRequestController() : base() {
            // Me.core = core
            Type myType = typeof(CoreController);
            Assembly myAssembly = Assembly.GetAssembly(myType);
            AssemblyName myAssemblyname = myAssembly.GetName();
            Version myVersion = myAssemblyname.Version;
            privateRequestTimeoutMsec = 30000;
            privateRequestUserAgent = "kmaHTTP/" + myVersion.Major.ToString("0") + "." + myVersion.Minor.ToString("00") + "." + myVersion.Build.ToString("00000000");
            http = new webClientExt();
        }
        //
        //============================================================================
        //
        //Public Function common_getHttpRequest(url As String) As Stream
        //    Dim returnstream As Stream = Nothing
        //    Try
        //        Dim rq As System.Net.WebRequest
        //        Dim response As System.Net.WebResponse
        //        '
        //        rq = System.Net.WebRequest.Create(url)
        //        rq.Timeout = 60000
        //        response = rq.GetResponse()
        //        returnstream = response.GetResponseStream()
        //    Catch ex As Exception
        //        Throw (ex)
        //    End Try
        //    Return returnstream
        //End Function
        //
        //======================================================================================
        //   Requests the doc and saves the body in the file specified
        //
        //   check the HTTPResponse and SocketResponse when done
        //   If the HTTPResponse is "", Check the SocketResponse
        //======================================================================================
        //
        public void getUrlToFile(string URL, string Filename) {
            try {
                string[] cookies = null;
                int CookiePointer = 0;
                string[] CookiePart = null;
                string path = null;
                int ptr = 0;
                //
                //core.AppendLog( "http4Class.getUrlToFile, url=[" & URL & "], filename=[" & Filename & "]")
                //
                privateResponseFilename = Filename;
                path = Filename.Replace("/", "\\");
                ptr = path.LastIndexOf("\\");
                if (ptr > 0) {
                    path = Filename.Left( ptr);
                    Directory.CreateDirectory(path);
                }
                File.Delete(privateResponseFilename);
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.UserAgent = privateRequestUserAgent;
                if (!string.IsNullOrEmpty(privateRequestCookie)) {
                    cookies = privateRequestCookie.Split(';');
                    for (CookiePointer = 0; CookiePointer <= cookies.GetUpperBound(0); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split('=');
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                }
                //
                privateRequestHeaders = http.Headers;
                http.Timeout = privateRequestTimeoutMsec;
                //
                privateRequestHeaders = http.Headers;
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                try {
                    http.DownloadFile(URL, privateResponseFilename);
                    //privateResponseProtocol = ""
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = 0;
                    if (File.Exists(privateResponseFilename)) {
                        privateResponseLength = (int)((new FileInfo(privateResponseFilename)).Length);
                    }
                    //Catch ex As WebException
                    //    '
                    //    ' http error, not 200
                    //    '
                    //    Dim we As WebException
                    //    Dim response As HttpWebResponse
                    //    we = DirectCast(ex, WebException)
                    //    response = we.Response
                    //    privateResponseStatusCode = response.StatusCode
                    //    privateResponseStatusDescription = response.StatusDescription
                    //    privateResponseHeaders = response.Headers
                    //    privateResponseLength = 0
                } catch {
                    //
                    //
                    //
                    privateResponseStatusCode = 0;
                    privateResponseStatusDescription = "";
                    privateResponseHeaders = new System.Net.WebHeaderCollection();
                    privateResponseLength = 0;
                    throw;
                }
                //http.DownloadFile(URL, privateResponseFilename)
                //privateResponseHeaders = http.ResponseHeaders
                //privateResponseLength = 0
                //If (File.Exists(privateResponseFilename)) Then
                //    privateResponseLength = (New FileInfo(privateResponseFilename)).Length
                //End If
            } catch (Exception ex) {
                //
                //
                //
                //Throw
                throw new httpException("Error in getUrlToFile(" + URL + "," + Filename + ")", ex);
            }
        }
        //
        //======================================================================================
        //   Returns the body of a URL requested
        //
        //   If there is an error, it returns "", and the HTTPResponse should be checked
        //   If the HTTPResponse is "", Check the SocketResponse
        //======================================================================================
        //
        public string getURL(ref string URL) {
            string returnString = "";
            try {
                string[] cookies = null;
                int CookiePointer = 0;
                string[] CookiePart = null;
                //
                //core.AppendLog( "http4Class.getURL, url=[" & URL & "]")
                //
                //Dim TimeoutTime As Date
                //
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.UserAgent = privateRequestUserAgent;
                if (!string.IsNullOrEmpty(privateRequestCookie)) {
                    cookies = privateRequestCookie.Split(';');
                    for (CookiePointer = 0; CookiePointer <= cookies.GetUpperBound(0); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split('=');
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                }
                http.Timeout = privateRequestTimeoutMsec;
                //TimeoutTime = System.DateTime.FromOADate(Now.ToOADate + (privateRequestTimeoutMsec / 24 / 60 / 60))
                //
                privateRequestHeaders = http.Headers;
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                privateResponseStatusCode = 0;
                privateResponseStatusDescription = "";
                //privateResponseHeaders = response.Headers
                privateResponseLength = 0;
                try {
                    returnString = http.DownloadString(URL);
                    //privateResponseProtocol = ""
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = returnString.Length;
                    //Catch ex As WebException
                    //    '
                    //    ' http error, not 200
                    //    '
                    //    Dim we As WebException
                    //    Dim response As HttpWebResponse
                    //    we = DirectCast(ex, WebException)
                    //    If Not (we.Response Is Nothing) Then
                    //        response = we.Response
                    //        privateResponseStatusCode = response.StatusCode
                    //        privateResponseStatusDescription = response.StatusDescription
                    //        privateResponseHeaders = response.Headers
                    //        privateResponseLength = CInt(response.ContentLength)
                    //    End If
                } catch {
                    //
                    //
                    //
                    throw;
                }
            } catch {
                //
                // general catch for the routine
                //
                throw;
            }
            //
            //core.AppendLog( "http4Class.getURL exit, return=[" & returnString & "]")
            //
            return returnString;
        }
        //
        //================================================================
        //
        //================================================================
        //
        public string userAgent {
            get {
                string returnString = "";
                try {
                    returnString = http.UserAgent;
                } catch  {
                    throw new ApplicationException("Error in UserAgent Property, get Method");
                }
                return returnString;
            }
            set {
                try {
                    http.UserAgent = value;
                } catch {
                    throw new ApplicationException("Error in UserAgent Property, set Method");
                }
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public int timeout {
            get {
                int returnTimeout = 0;
                try {
                    returnTimeout = encodeInteger(http.Timeout / 1000);
                } catch {
                    throw new ApplicationException("Error in Timeout Property, get Method");
                }
                return returnTimeout;
            }
            set {
                try {
                    if (value > 65535) {
                        value = 65535;
                    }
                    http.Timeout = value * 1000;
                } catch {
                    throw new ApplicationException("Error in Timeout Property, set Method");
                }
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public string requestHeader {
            get {
                string returnString = "";
                int ptr = 0;
                //
                try {
                    if (privateRequestHeaders.Count > 0) {
                        for (ptr = 0; ptr < privateRequestHeaders.Count; ptr++) {
                            returnString += privateRequestHeaders[ptr];
                        }
                    }
                } catch {
                    throw new ApplicationException("Error in requestHeader Property, get Method");
                }
                return returnString;
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public string responseHeader {
            get {
                string returnString = "";
                int ptr = 0;
                //
                try {
                    if (privateResponseStatusCode != 0) {
                        returnString += privateResponseProtocol + " " + privateResponseStatusCode + " " + privateResponseStatusDescription;
                        if (privateResponseHeaders.Count > 0) {
                            for (ptr = 0; ptr < privateResponseHeaders.Count; ptr++) {
                                returnString += "\r\n" + privateResponseHeaders.GetKey(ptr) + ":" + privateResponseHeaders[ptr];
                            }
                        }
                    }
                } catch {
                    throw;
                    //Throw New ApplicationException("Error in responseHeader Property, get Method")
                }
                return returnString;
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public string socketResponse {
            get {
                string returnString = "";
                //Dim ptr As Integer
                //
                try {
                    returnString = privateSocketResponse;
                } catch {
                    throw new ApplicationException("Error in SocketResponse Property, get Method");
                }
                return returnString;
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        //Public ReadOnly Property responseLength() As String
        //    Get
        //        Dim returnString As String = ""
        //        'Dim ptr As Integer
        //        '
        //        Try
        //            returnString = privateResponseLength
        //        Catch ex As Exception
        //            Throw New ApplicationException("Error in ResponseLength Property, get Method")
        //        End Try
        //        Return returnString
        //    End Get
        //End Property
        //
        //================================================================
        //
        //================================================================
        //
        public string responseStatusDescription {
            get {
                string returnString = "";
                //Dim ptr As Integer
                //
                try {
                    returnString = privateResponseStatusDescription;
                } catch {
                    throw;
                }
                return returnString;
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public int responseStatusCode {
            get {
                int returnCode = 0;
                //Dim ptr As Integer
                //
                try {
                    returnCode = privateResponseStatusCode;
                } catch  {
                    throw;
                }
                return returnCode;
            }
        }
        //
        //
        //
        public string setCookie {
            set {
                try {
                    privateRequestCookie = value;
                } catch  {
                    throw;
                }
            }
        }
        //
        //
        //
        public string username {
            set {
                try {
                    privateRequestUsername = value;
                } catch  {
                    throw;
                }
            }
        }
        //
        //
        //
        public string password {
            set {
                try {
                    privateRequestPassword = value;
                } catch {
                    throw;
                }
            }
        }
        //
        ~httpRequestController() {
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
            http.Dispose();
        }
        //
        //
        //
    }
    //
    // exception classes
    //
    public class httpException : ApplicationException {
        //
        public httpException(string context, Exception innerEx) : base("Unknown error in http4Class, " + context + ", innerException [" + innerEx.ToString() + "]") {
        }
    }

}

