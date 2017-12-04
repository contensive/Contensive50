

using System.IO;
using System.Net;
// 
const object DebugBuild = true;

namespace Controllers {
    
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
        
        private string privateResponseProtocol = "HTTP/1.1";
        
        private string privateResponseStatusDescription;
        
        private int privateResponseStatusCode;
        
        // Private privateResponseStatus As System.Net.HttpStatusCode = New System.Net.HttpStatusCode
        private System.Net.WebHeaderCollection privateResponseHeaders = new System.Net.WebHeaderCollection();
        
        private int privateResponseLength = 0;
        
        // 
        private string privateSocketResponse = "";
        
        public httpRequestController() {
            //  Me.cpcore = cpcore
            privateRequestTimeoutMsec = 30000;
            privateRequestUserAgent = ("kmaHTTP/" 
                        + (My.Application.Info.Version.Major + ("." + My.Application.Info.Version.Minor)));
            http = new webClientExt();
        }
        
        // '
        // '============================================================================
        // '
        // Public Function common_getHttpRequest(url As String) As IO.Stream
        //     Dim returnstream As IO.Stream = Nothing
        //     Try
        //         Dim rq As System.Net.WebRequest
        //         Dim response As System.Net.WebResponse
        //         '
        //         rq = System.Net.WebRequest.Create(url)
        //         rq.Timeout = 60000
        //         response = rq.GetResponse()
        //         returnstream = response.GetResponseStream()
        //     Catch ex As Exception
        //         Throw (ex)
        //     End Try
        //     Return returnstream
        // End Function
        // 
        // ======================================================================================
        //    Requests the doc and saves the body in the file specified
        // 
        //    check the HTTPResponse and SocketResponse when done
        //    If the HTTPResponse is "", Check the SocketResponse
        // ======================================================================================
        // 
        public void getUrlToFile(ref string URL, ref string Filename) {
            try {
                string[] cookies;
                int CookiePointer;
                string[] CookiePart;
                string path;
                int ptr;
                // 
                // cpCore.AppendLog( "http4Class.getUrlToFile, url=[" & URL & "], filename=[" & Filename & "]")
                // 
                privateResponseFilename = Filename;
                path = Filename.Replace("/", "\\");
                ptr = path.LastIndexOf("\\");
                if ((ptr > 0)) {
                    path = Filename.Substring(0, ptr);
                    Directory.CreateDirectory(path);
                }
                
                File.Delete(privateResponseFilename);
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.UserAgent = privateRequestUserAgent;
                if ((privateRequestCookie != "")) {
                    cookies = privateRequestCookie.Split(";");
                    for (CookiePointer = 0; (CookiePointer <= UBound(cookies)); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split("=");
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                    
                }
                
                // 
                privateRequestHeaders = http.Headers();
                http.Timeout = privateRequestTimeoutMsec;
                // 
                privateRequestHeaders = http.Headers();
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                try {
                    http.DownloadFile(URL, privateResponseFilename);
                    // privateResponseProtocol = ""
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = 0;
                    if (File.Exists(privateResponseFilename)) {
                        privateResponseLength = int.Parse(new FileInfo(privateResponseFilename).Length);
                    }
                    
                    // Catch ex As WebException
                    //     '
                    //     ' http error, not 200
                    //     '
                    //     Dim we As WebException
                    //     Dim response As HttpWebResponse
                    //     we = DirectCast(ex, WebException)
                    //     response = we.Response
                    //     privateResponseStatusCode = response.StatusCode
                    //     privateResponseStatusDescription = response.StatusDescription
                    //     privateResponseHeaders = response.Headers
                    //     privateResponseLength = 0
                }
                catch (Exception ex) {
                    // 
                    // 
                    // 
                    privateResponseStatusCode = 0;
                    privateResponseStatusDescription = "";
                    privateResponseHeaders = new System.Net.WebHeaderCollection();
                    privateResponseLength = 0;
                    throw;
                }
                
                // http.DownloadFile(URL, privateResponseFilename)
                // privateResponseHeaders = http.ResponseHeaders
                // privateResponseLength = 0
                // If (File.Exists(privateResponseFilename)) Then
                //     privateResponseLength = (New FileInfo(privateResponseFilename)).Length
                // End If
            }
            catch (Exception ex) {
                // 
                // 
                // 
                // Throw
                throw new httpException(("Error in getUrlToFile(" 
                                + (URL + ("," 
                                + (Filename + ")")))), ex);
            }
            
        }
        
        // 
        // ======================================================================================
        //    Returns the body of a URL requested
        // 
        //    If there is an error, it returns "", and the HTTPResponse should be checked
        //    If the HTTPResponse is "", Check the SocketResponse
        // ======================================================================================
        // 
        public string getURL(ref string URL) {
            string returnString = "";
            try {
                string[] cookies;
                int CookiePointer;
                string[] CookiePart;
                // 
                // cpCore.AppendLog( "http4Class.getURL, url=[" & URL & "]")
                // 
                // Dim TimeoutTime As Date
                // 
                http.password = privateRequestPassword;
                http.username = privateRequestUsername;
                http.UserAgent = privateRequestUserAgent;
                if ((privateRequestCookie != "")) {
                    cookies = privateRequestCookie.Split(";");
                    for (CookiePointer = 0; (CookiePointer <= UBound(cookies)); CookiePointer++) {
                        CookiePart = cookies[CookiePointer].Split("=");
                        http.addCookie(CookiePart[0], CookiePart[1]);
                    }
                    
                }
                
                http.Timeout = privateRequestTimeoutMsec;
                // TimeoutTime = System.DateTime.FromOADate(Now.ToOADate + (privateRequestTimeoutMsec / 24 / 60 / 60))
                // 
                privateRequestHeaders = http.Headers();
                privateResponseHeaders = new System.Net.WebHeaderCollection();
                privateResponseLength = 0;
                privateResponseStatusCode = 0;
                privateResponseStatusDescription = "";
                privateResponseLength = 0;
                try {
                    returnString = http.DownloadString(URL);
                    // privateResponseProtocol = ""
                    privateResponseStatusCode = 200;
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString();
                    privateResponseHeaders = http.ResponseHeaders;
                    privateResponseLength = returnString.Length;
                    // Catch ex As WebException
                    //     '
                    //     ' http error, not 200
                    //     '
                    //     Dim we As WebException
                    //     Dim response As HttpWebResponse
                    //     we = DirectCast(ex, WebException)
                    //     If Not (we.Response Is Nothing) Then
                    //         response = we.Response
                    //         privateResponseStatusCode = response.StatusCode
                    //         privateResponseStatusDescription = response.StatusDescription
                    //         privateResponseHeaders = response.Headers
                    //         privateResponseLength = CInt(response.ContentLength)
                    //     End If
                }
                catch (Exception ex) {
                    // 
                    // 
                    // 
                    throw;
                }
                catch (System.Exception Throw) {
                }
                
                // 
                // cpCore.AppendLog( "http4Class.getURL exit, return=[" & returnString & "]")
                // 
                return returnString;
            }
            
            // 
            // ================================================================
            // 
            // ================================================================
            // 
        }
        
        string userAgent {
            get {
                string returnString = "";
                try {
                    returnString = http.UserAgent;
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in UserAgent Property, get Method");
                }
                
                return returnString;
            }
            set {
                try {
                    http.UserAgent = value;
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in UserAgent Property, set Method");
                }
                
            }
        }
        
        public int timeout {
            get {
                int returnTimeout = 0;
                try {
                    returnTimeout = int.Parse((http.Timeout / 1000));
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in Timeout Property, get Method");
                }
                
                return returnTimeout;
            }
            set {
                try {
                    if ((value > 65535)) {
                        value = 65535;
                    }
                    
                    http.Timeout = (value * 1000);
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in Timeout Property, set Method");
                }
                
            }
        }
        
        public string requestHeader {
            get {
                string returnString = "";
                int ptr;
                // 
                try {
                    if ((privateRequestHeaders.Count > 0)) {
                        for (ptr = 0; (ptr 
                                    <= (privateRequestHeaders.Count - 1)); ptr++) {
                            privateRequestHeaders.Item[ptr];
                        }
                        
                    }
                    
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in requestHeader Property, get Method");
                }
                
                return returnString;
            }
        }
        
        public string responseHeader {
            get {
                string returnString = "";
                int ptr;
                // 
                try {
                    if ((privateResponseStatusCode != 0)) {
                        (privateResponseProtocol + (" " 
                                    + (privateResponseStatusCode + (" " + privateResponseStatusDescription))));
                        if ((privateResponseHeaders.Count > 0)) {
                            for (ptr = 0; (ptr 
                                        <= (privateResponseHeaders.Count - 1)); ptr++) {
                                ("\r\n" 
                                            + (privateResponseHeaders.GetKey(ptr) + (":" + privateResponseHeaders.Item[ptr])));
                            }
                            
                        }
                        
                    }
                    
                }
                catch (Exception ex) {
                    throw;
                }
                
                return returnString;
            }
        }
        
        public string socketResponse {
            get {
                string returnString = "";
                try {
                    returnString = privateSocketResponse;
                }
                catch (Exception ex) {
                    throw new ApplicationException("Error in SocketResponse Property, get Method");
                }
                
                return returnString;
            }
        }
        
        public string responseStatusDescription {
            get {
                string returnString = "";
                try {
                    returnString = privateResponseStatusDescription;
                }
                catch (Exception ex) {
                    throw;
                }
                
                return returnString;
            }
        }
        
        public int responseStatusCode {
            get {
                int returnCode = 0;
                // Dim ptr As Integer
                // 
                try {
                    returnCode = privateResponseStatusCode;
                }
                catch (Exception ex) {
                    throw;
                }
                
                return returnCode;
            }
        }
        
        public string setCookie {
            set {
                try {
                    privateRequestCookie = value;
                }
                catch (Exception ex) {
                    throw;
                }
                
            }
        }
        
        public string username {
            set {
                try {
                    privateRequestUsername = value;
                }
                catch (Exception ex) {
                    throw;
                }
                
            }
        }
        
        public string password {
            set {
                try {
                    privateRequestPassword = value;
                }
                catch (Exception ex) {
                    throw;
                }
                
            }
        }
        
        protected override void Finalize() {
            base.Finalize();
            http.Dispose();
        }
    }
    
    // 
    //  exception classes
    // 
    public class httpException : ApplicationException {
        
        // 
        public httpException(string context, Exception innerEx) : 
                base(("Unknown error in http4Class, " 
                                + (context + (", innerException [" 
                                + (innerEx.ToString() + "]"))))) {
        }
    }
}