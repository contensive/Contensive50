//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//

//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPRequestBaseClass
	{
		//Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
		public abstract string Browser {get;} //Implements BaseClasses.CPRequestBaseClass.Browser
		public abstract bool BrowserIsIE {get;} //Implements BaseClasses.CPRequestBaseClass.BrowserIsIE
		public abstract bool BrowserIsMac {get;} //Implements BaseClasses.CPRequestBaseClass.BrowserIsMac
		public abstract bool BrowserIsMobile {get;} //Implements BaseClasses.CPRequestBaseClass.BrowserIsMobile
		public abstract bool BrowserIsWindows {get;} //Implements BaseClasses.CPRequestBaseClass.BrowserIsWindows
		public abstract string BrowserVersion {get;} //Implements BaseClasses.CPRequestBaseClass.BrowserVersion
		public abstract string Cookie(string CookieName); //Implements BaseClasses.CPRequestBaseClass.Cookie
		public abstract string CookieString {get;} //Implements BaseClasses.CPRequestBaseClass.CookieString
		public abstract string Form {get;} //Implements BaseClasses.CPRequestBaseClass.Form
		public abstract string FormAction {get;} //Implements BaseClasses.CPRequestBaseClass.FormAction
		public abstract bool GetBoolean(string RequestName); //Implements BaseClasses.CPRequestBaseClass.GetBoolean
		public abstract DateTime GetDate(string RequestName); //Implements BaseClasses.CPRequestBaseClass.GetDate
		public abstract int GetInteger(string RequestName); //Implements BaseClasses.CPRequestBaseClass.GetInteger
		public abstract double GetNumber(string RequestName); //Implements BaseClasses.CPRequestBaseClass.GetNumber
		public abstract string GetText(string RequestName); //Implements BaseClasses.CPRequestBaseClass.GetText
		public abstract string Host {get;} //Implements BaseClasses.CPRequestBaseClass.Host
		public abstract string HTTPAccept {get;} //Implements BaseClasses.CPRequestBaseClass.HTTPAccept
		public abstract string HTTPAcceptCharset {get;} //Implements BaseClasses.CPRequestBaseClass.HTTPAcceptCharset
		public abstract string HTTPProfile {get;} //Implements BaseClasses.CPRequestBaseClass.HTTPProfile
		public abstract string HTTPXWapProfile {get;} //Implements BaseClasses.CPRequestBaseClass.HTTPXWapProfile
		public abstract string Language {get;} //Implements BaseClasses.CPRequestBaseClass.Language
		public abstract string Link {get;} //Implements BaseClasses.CPRequestBaseClass.Link
		public abstract string LinkForwardSource {get;} //Implements BaseClasses.CPRequestBaseClass.LinkForwardSource
		public abstract string LinkSource {get;} //Implements BaseClasses.CPRequestBaseClass.LinkSource
		public abstract string Page {get;} //Implements BaseClasses.CPRequestBaseClass.Page
		public abstract string Path {get;} //Implements BaseClasses.CPRequestBaseClass.Path
		public abstract string PathPage {get;} //Implements BaseClasses.CPRequestBaseClass.PathPage
		public abstract string Protocol {get;} //Implements BaseClasses.CPRequestBaseClass.Protocol
		public abstract string QueryString {get;} //Implements BaseClasses.CPRequestBaseClass.QueryString
		public abstract string Referer {get;} //Implements BaseClasses.CPRequestBaseClass.Referer
		public abstract string RemoteIP {get;} //Implements BaseClasses.CPRequestBaseClass.RemoteIP
		public abstract bool Secure {get;} //Implements BaseClasses.CPRequestBaseClass.Secure
		public abstract bool OK(string RequestName); //Implements BaseClasses.CPRequestBaseClass.OK
	}

}

