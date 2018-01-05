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
	public abstract class CPResponseBaseClass
	{
		public abstract string ContentType {get; set;}  
		public abstract string Cookies {get;} 
		public abstract string Header {get;} 
		public abstract void Clear(); 
		public abstract void Close();  
		public abstract void AddHeader(string HeaderName, string HeaderValue);  
		public abstract void Flush();  
		public abstract void Redirect(string Link);  
		public abstract void SetBuffer(bool BufferOn); 
		public abstract void SetStatus(string status); 
		public abstract void SetTimeout(string TimeoutSeconds);  
		public abstract void SetType(string ContentType); 
        public abstract void SetCookie(string CookieName, string CookieValue);
        public abstract void SetCookie(string CookieName, string CookieValue, DateTime DateExpires);
        public abstract void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain);
        public abstract void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path);
        public abstract void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain , string Path , bool Secure );
        public abstract void Write(string content);
		/// <summary>
		/// Is the response object available to write. False during background processes and after the html response has ended. For instance, when a remote method is returned the response is closed meaning no other data should be added to the output.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool isOpen {get;}
	}

}

