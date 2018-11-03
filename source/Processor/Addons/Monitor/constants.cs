
//using System;

//namespace Contensive.Addons.Monitor {
//    public static class constants {
//        //
//        // Global Configuration Variables
//        //   Read in from config file each timer hit and on start
//        //'
//        public struct AppLogType {
//            public string Name;
//            public int StatusCheckCount;
//            public int ErrorCount;
//            public string SiteLink;
//            public string StatusLink;
//            public string LastStatusResponse;
//            public bool LastCheckOK; // true if the last check was OK
//        }
//        //
//        public struct ConnType {
//            public int ConnectionID;
//            public int BytesToSend;
//            public string HTTPVersion;
//            public string HTTPMethod;
//            public string Path;
//            public string Query;
//            public string Headers;
//            public string PostData;
//            public bool DataReadyToSend;
//            public int State;
//            public int ContentLength;
//            public DateTime dateOpened;
//            public string sourceIp;
//        }
//        //
//        // Socket Request Block
//        //
//        public struct RobotPathType {
//            public string Agent;
//            public string Path;
//        }
//        //
//        public struct DocType {
//            // Initialize before socket call
//            public string URI; // <scheme>://<user>:<Password>@<host>:<Port>/<url-path>?<Query>
//            public string URIScheme; // http,ftp, etc. (semi supported)
//            public string URIUser; // (not supported yet)
//            public string URIPassword; // (not supported yet)
//            public string URIPort; // (not supported yet)
//            public string URIHost;
//            public string URIPath;
//            public string URIPage;
//            public string URIQuery; // (not supported yet - stuck to path)
//            public int Id; // ID in database for this doc record
//            public bool DontGet; // if true, do not request or analyze the document
//                                 // values set by socket
//            public string SocketResponse; // Socket Response (if not "", socket error)
//            public string HTTPResponse; // HTTP response (version,code,description)
//            public string HTTPResponseCode; // HTTP response Code (200, etc)
//            public double ResponseTime; // time to fetch this page
//            public string RequestFilename; // the client request (only saved if testing)
//            public string ResponseFilename; // the server response
//            public int ResponseFileNumber; // filenumber for the response file
//            public int EntityStart; // character count of the first byte of entity in the response file
//            public int ResponseFileLength; // length of content read in from HTTP
//            public int RetryCountAuth; // retires for authorization
//            public string TextOnlyFilename;
//            public int RetryCountTimeout; // retires for timeouts
//                                          // Set by .
//            public bool Found; // if true, doc was found and read
//            public bool OffSite; // true if URI is not on Host being tested, set during .
//            public bool HTML; // content_type is html/text
//            public string Title;
//            public string MetaKeywords;
//            public string MetaDescription;
//            public string UnknownWordList; // List not found by spell checker
//                                           // accumulated errors and warnings
//            public int ErrorCount; // count of site errors
//        }
//        //
//        // Task value defaults
//        //
//        public const bool URLOnSiteDefault = false;
//        public const int DocLinkCountMaxDefault = 1000;
//        public const int URIRootLimitDefault = 1;
//        public const int HonorRobotsDefault = 1;
//        public const int CookiesPageToPageDefault = 1;
//        public const int CookiesVisitToVisitDefault = 1;
//        public const string AuthUsernameDefault = "";
//        public const string AuthPasswordDefault = "";
//    }
//}

