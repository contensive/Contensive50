using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using System.Runtime.InteropServices;

namespace Contensive.Core
{
	//
	// comVisible to be activeScript compatible
	//
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPRequestClass.ClassId, CPRequestClass.InterfaceId, CPRequestClass.EventsId)]
	public class CPRequestClass : BaseClasses.CPRequestBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "EF7782C1-76E4-45A7-BF30-E1CEBCBC56CF";
		public const string InterfaceId = "39D6A73F-C11A-44F4-8405-A4CE3FB0A486";
		public const string EventsId = "C8938AB2-26F0-41D2-A282-3313FD7BA490";
#endregion
		//
		private Contensive.Core.coreClass cpCore;
		protected bool disposed = false;
		//
		// Constructor
		//
		public CPRequestClass(Contensive.Core.coreClass cpCoreObj) : base()
		{
			cpCore = cpCoreObj;
		}
		//
		// dispose
		//
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				appendDebugLog(".dispose, dereference main, csv");
				if (disposing)
				{
					//
					// call .dispose for managed objects
					//
					cpCore = null;
				}
				//
				// Add code here to release the unmanaged resource.
				//
			}
			this.disposed = true;
		}

		public override string Browser
		{
			get
			{
				if (true)
				{
					return cpCore.webServer.requestBrowser;
				}
				else
				{
					return "";
				}
			}
		}

		public override bool BrowserIsIE
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit_browserIsIE;
				}
				else
				{
					return false;
				}
			}
		}

		public override bool BrowserIsMac
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit_browserIsMac;
				}
				else
				{
					return false;
				}
			}
		}

		public override bool BrowserIsMobile
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.Mobile;
				}
				else
				{
					return false;
				}
			}
		}

		public override bool BrowserIsWindows
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit_browserIsWindows;
				}
				else
				{
					return false;
				}
			}
		}

		public override string BrowserVersion
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit_browserVersion;
				}
				else
				{
					return "";
				}
			}
		}

		public override string Cookie(string CookieName)
		{
			if (true)
			{
				return cpCore.webServer.getRequestCookie(CookieName);
			}
			else
			{
				return "";
			}
		}
		//====================================================================================================
		/// <summary>
		/// return a string that includes the simple name value pairs for all request cookies
		/// </summary>
		/// <returns></returns>
		public override string CookieString
		{
			get
			{
				string returnCookies = "";
				foreach (KeyValuePair<string, iisController.cookieClass> kvp in cpCore.webServer.requestCookies)
				{
					returnCookies += "&" + kvp.Key + "=" + kvp.Value.value;
				}
				if (returnCookies.Length > 0)
				{
					returnCookies = returnCookies.Substring(1);
				}
				return returnCookies;
			}
		}

		public override string Form
		{
			get
			{
				return Controllers.genericController.convertNameValueDictToREquestString(cpCore.webServer.requestFormDict);
			}
		}

		public override string FormAction
		{
			get
			{
				return cpCore.webServer.serverFormActionURL;
			}
		}

		public override bool GetBoolean(string RequestName)
		{
			return cpCore.docProperties.getBoolean(RequestName);
		}

		public override DateTime GetDate(string RequestName)
		{
			return cpCore.docProperties.getDate(RequestName);
		}

		public override int GetInteger(string RequestName)
		{
			return cpCore.docProperties.getInteger(RequestName);
		}

		public override double GetNumber(string RequestName)
		{
			return cpCore.docProperties.getNumber(RequestName);
		}

		public override string GetText(string RequestName)
		{
			return cpCore.docProperties.getText(RequestName);
		}

		public override string Host
		{
			get
			{
				return cpCore.webServer.requestDomain;
			}
		}

		public override string HTTPAccept
		{
			get
			{
				return cpCore.webServer.requestHttpAccept;
			}
		}

		public override string HTTPAcceptCharset
		{
			get
			{
				return cpCore.webServer.requestHttpAcceptCharset;
			}
		}

		public override string HTTPProfile
		{
			get
			{
				return cpCore.webServer.requestHttpProfile;
			}
		}

		public override string HTTPXWapProfile
		{
			get
			{
				return cpCore.webServer.requestxWapProfile;
			}
		}

		public override string Language
		{
			get
			{
				if (cpCore.doc.authContext.userLanguage == null)
				{
					return "";
				}
				Models.Entity.LanguageModel userLanguage = Models.Entity.LanguageModel.create(cpCore, cpCore.doc.authContext.user.LanguageID, new List<string>());
				if (userLanguage != null)
				{
					return userLanguage.Name;
				}
				return "English";
			}
		}

		public override string Link
		{
			get
			{
				return cpCore.webServer.requestUrl;
			}
		}

		public override string LinkForwardSource
		{
			get
			{
				return cpCore.webServer.linkForwardSource;
			}
		}

		public override string LinkSource
		{
			get
			{
				return cpCore.webServer.requestUrlSource;
			}
		}

		public override string Page
		{
			get
			{
				return cpCore.webServer.requestPage;
			}
		}

		public override string Path
		{
			get
			{
				return cpCore.webServer.requestPath;
			}
		}

		public override string PathPage
		{
			get
			{
				return cpCore.webServer.requestPathPage;
			}
		}

		public override string Protocol
		{
			get
			{
				return cpCore.webServer.requestProtocol;
			}
		}

		public override string QueryString
		{
			get
			{
				return cpCore.webServer.requestQueryString;
			}
		}

		public override string Referer
		{
			get
			{
				return cpCore.webServer.requestReferer;
			}
		}

		public override string RemoteIP
		{
			get
			{
				return cpCore.webServer.requestRemoteIP;
			}
		}

		public override bool Secure
		{
			get
			{
				return cpCore.webServer.requestSecure;
			}
		}

		public override bool OK(string RequestName)
		{
			return cpCore.docProperties.containsKey(RequestName);
		}
		//
		//
		//
		private void appendDebugLog(string copy)
		{
			//My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.request, " & copy & vbCrLf, True)
			// 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
		}
		//
		// testpoint
		//
		private void tp(string msg)
		{
			//Call appendDebugLog(msg)
		}
#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPRequestClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}
}