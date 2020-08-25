//========================================================================



//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPRequestBaseClass
	{
		public abstract string Browser {get;}
        [Obsolete("Deprecated",false)] public abstract bool BrowserIsIE {get;}
		[Obsolete("Deprecated",false)] public abstract bool BrowserIsMac {get;}
		public abstract bool BrowserIsMobile {get;}
        [Obsolete("Deprecated", false)] public abstract bool BrowserIsWindows {get;}
        [Obsolete("Deprecated", false)] public abstract string BrowserVersion {get;}
		public abstract string Cookie(string CookieName); 
		public abstract string CookieString {get;} 
		public abstract string Form {get;} 
		public abstract string FormAction {get;} 
		public abstract bool GetBoolean(string RequestName); 
		public abstract DateTime GetDate(string RequestName);
		public abstract int GetInteger(string RequestName);
		public abstract double GetNumber(string RequestName);
		public abstract string GetText(string RequestName);
		public abstract string Host {get;}
		public abstract string HTTPAccept {get;}
		public abstract string HTTPAcceptCharset {get;}
		public abstract string HTTPProfile {get;} 
		public abstract string HTTPXWapProfile {get;}
		public abstract string Language {get;}
		public abstract string Link {get;}
		public abstract string LinkForwardSource {get;}
		public abstract string LinkSource {get;}
		public abstract string Page {get;}
		public abstract string Path {get;}
		public abstract string PathPage {get;}
		public abstract string Protocol {get;}
		public abstract string QueryString {get;}
		public abstract string Referer {get;}
		public abstract string RemoteIP {get;}
		public abstract bool Secure {get;}
		public abstract bool OK(string RequestName);
        public abstract string Body { get; }
        public abstract string ContentType { get; }
    }

}

