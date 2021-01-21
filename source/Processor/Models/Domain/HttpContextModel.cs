using Contensive.BaseModels;
using System;
using System.Collections.Generic;

namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// Parameter object for CPClass to provide website request context
    /// </summary>
    public class HttpContextModel : HttpContextBaseModel {
        /// <summary>
        /// Request
        /// </summary>
        public HttpContextRequest Request;
        /// <summary>
        /// Response
        /// </summary>
        public HttpContextResponse Response;
        /// <summary>
        /// 
        /// </summary>
        public HttpContextApplicationInstance ApplicationInstance;

    }
    //
    //====================================================================================================
    /// <summary>
    /// 
    /// </summary>
    public class HttpContextApplicationInstance {
        /// <summary>
        /// 
        /// </summary>
        public void CompleteRequest() {

        }
    }
    //
    //====================================================================================================
    /// <summary>
    /// Request Object
    /// </summary>
    public class HttpContextRequest {
        /// <summary>
        /// Request Url
        /// </summary>
        public HttpContentRequestUrl Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Uri UrlReferrer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, HttpContextRequestCookie> Cookies { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> Form { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> QueryString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string,string> Headers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string,string> ServerVariables { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// The content body created before the call
        /// </summary>
        public string requestBody { get; set; }
    }
    //
    //====================================================================================================
    /// <summary>
    /// Request Url
    /// </summary>
    public class HttpContentRequestUrl {
        /// <summary>
        /// port
        /// </summary>
        public int Port;
        /// <summary>
        /// 
        /// </summary>
        public string AbsoluteUri;
    }
    /// <summary>
    /// 
    /// </summary>
    public class HttpContextRequestCookie {
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public string Value;
    }
    //
    //====================================================================================================
    /// <summary>
    /// 
    /// </summary>
    public class HttpContextResponse {
        /// <summary>
        /// 
        /// </summary>
        public void ClearHeaders() {

        }
        /// <summary>
        /// 
        /// </summary>
        public void Flush() {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Redirect( string a, bool b) {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="HeaderName"></param>
        /// <param name="HeaderValue"></param>
        public void AddHeader(string HeaderName, string HeaderValue) {

        }
        /// <summary>
        /// 
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set;  }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, HttpContextResponseCookie> Cookies;
        /// <summary>
        /// 
        /// </summary>
        public string CacheControl;
        /// <summary>
        /// 
        /// </summary>
        public int Expires;
        /// <summary>
        /// 
        /// </summary>
        public bool Buffer;
    }
    /// <summary>
    /// 
    /// </summary>
    public class HttpContextResponseCookie {
        /// <summary>
        /// 
        /// </summary>
        public bool HttpOnly;
        /// <summary>
        /// 
        /// </summary>
        public HttpContextResponseCookieSameSiteMode SameSite;
        /// <summary>
        /// 
        /// </summary>
        public string Value;
        /// <summary>
        /// 
        /// </summary>
        public DateTime Expires;
        /// <summary>
        /// 
        /// </summary>
        public string Domain;
        /// <summary>
        /// 
        /// </summary>
        public string Path;
        /// <summary>
        /// 
        /// </summary>
        public bool Secure;
    }
    /// <summary>
    /// 
    /// </summary>
    public enum HttpContextResponseCookieSameSiteMode {
        /// <summary>
        /// 
        /// </summary>
        Lax = 1
    }
}

