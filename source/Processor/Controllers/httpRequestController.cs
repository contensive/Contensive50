
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
// todo -- this should use the filesystem objects, not system.io
using System.IO;
using System.Net;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Controllers {
    //
    public class HttpRequestController {
        //
        private WebClientExt http;
        private System.Net.WebHeaderCollection privateRequestHeaders;
        private string privateRequestPassword;
        private string privateRequestUsername;
        private readonly string privateRequestUserAgent;
        private string privateRequestCookie;
        private readonly int privateRequestTimeoutMsec;
        private string privateResponseFilename;
        private readonly string privateResponseProtocol = "HTTP/1.1"; // had to fake bc webClient removes first line of header
        private string privateResponseStatusDescription;
        private int privateResponseStatusCode;
        private WebHeaderCollection privateResponseHeaders = new System.Net.WebHeaderCollection();
        private int privateResponseLength = 0;
        private readonly string privateSocketResponse = "";
        //
        //======================================================================================
        // constructor
        //======================================================================================
        //
        public HttpRequestController() {
            // Me.core = core
            Type myType = typeof(CoreController);
            Assembly myAssembly = Assembly.GetAssembly(myType);
            AssemblyName myAssemblyname = myAssembly.GetName();
            Version myVersion = myAssemblyname.Version;
            privateRequestTimeoutMsec = 30000;
            privateRequestUserAgent = "kmaHTTP/" + myVersion.Major.ToString("0") + "." + myVersion.Minor.ToString("00") + "." + myVersion.Build.ToString("00000000");
            http = new WebClientExt();
        }
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
                privateResponseFilename = Filename;
                path = Filename.Replace("/", "\\");
                ptr = path.LastIndexOf("\\");
                if (ptr > 0) {
                    path = Filename.left( ptr);
                    Directory.CreateDirectory(path);
                }
                File.Delete(privateResponseFilename);
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.userAgent = privateRequestUserAgent;
                if (!string.IsNullOrEmpty(privateRequestCookie)) {
                    cookies = privateRequestCookie.Split(';');
                    for (CookiePointer = 0; CookiePointer <= cookies.GetUpperBound(0); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split('=');
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                }
                //
                privateRequestHeaders = http.Headers;
                http.timeout = privateRequestTimeoutMsec;
                //
                privateRequestHeaders = http.Headers;
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                try {
                    http.DownloadFile(URL, privateResponseFilename);
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = 0;
                    if (File.Exists(privateResponseFilename)) {
                        privateResponseLength = (int)((new FileInfo(privateResponseFilename)).Length);
                    }
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
            } catch (Exception ex) {
                throw new HttpException("Error in getUrlToFile(" + URL + "," + Filename + ")", ex);
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
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.userAgent = privateRequestUserAgent;
                if (!string.IsNullOrEmpty(privateRequestCookie)) {
                    cookies = privateRequestCookie.Split(';');
                    for (CookiePointer = 0; CookiePointer <= cookies.GetUpperBound(0); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split('=');
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                }
                http.timeout = privateRequestTimeoutMsec;
                //
                privateRequestHeaders = http.Headers;
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                privateResponseStatusCode = 0;
                privateResponseStatusDescription = "";
                privateResponseLength = 0;
                try {
                    returnString = http.DownloadString(URL);
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = returnString.Length;
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
                    returnString = http.userAgent;
                } catch  {
                    throw new GenericException("Error in UserAgent Property, get Method");
                }
                return returnString;
            }
            set {
                try {
                    http.userAgent = value;
                } catch {
                    throw new GenericException("Error in UserAgent Property, set Method");
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
                    returnTimeout = encodeInteger(http.timeout / 1000);
                } catch {
                    throw new GenericException("Error in Timeout Property, get Method");
                }
                return returnTimeout;
            }
            set {
                try {
                    if (value > 65535) {
                        value = 65535;
                    }
                    http.timeout = value * 1000;
                } catch {
                    throw new GenericException("Error in Timeout Property, set Method");
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
                    throw new GenericException("Error in requestHeader Property, get Method");
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
                                returnString += Environment.NewLine + privateResponseHeaders.GetKey(ptr) + ":" + privateResponseHeaders[ptr];
                            }
                        }
                    }
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
        public string socketResponse {
            get {
                string returnString = "";
                //
                try {
                    returnString = privateSocketResponse;
                } catch {
                    throw new GenericException("Error in SocketResponse Property, get Method");
                }
                return returnString;
            }
        }
        //
        //================================================================
        //
        //================================================================
        //
        public string responseStatusDescription {
            get {
                string returnString = "";
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
        ~HttpRequestController() {
            
            
            http.Dispose();
        }
        //
        //
        //
    }
    //
    // exception classes
    //
    public class HttpException : ApplicationException {
        //
        public HttpException(string context, Exception innerEx) : base("Unknown error in http4Class, " + context + ", innerException [" + innerEx.ToString() + "]") {
        }
    }

}

