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
	//
	//====================================================================================================
	/// <summary>
	/// Application settings and methods
	/// </summary>
	public abstract class CPSiteBaseClass
	{
		//
		//====================================================================================================
		//
		public abstract string Name {get;}
		//
		//====================================================================================================
		//
		public abstract void SetProperty(string FieldName, string FieldValue);
		//
		//====================================================================================================
		//
		public abstract string GetProperty(string FieldName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract string GetText(string PropertyName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract bool GetBoolean(string PropertyName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract DateTime GetDate(string PropertyName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract int GetInteger(string PropertyName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract double GetNumber(string PropertyName, string DefaultValue = "");
		//
		//====================================================================================================
		//
		public abstract bool MultiDomainMode {get;}
		//
		//====================================================================================================
		//
		public abstract string PhysicalFilePath {get;}
		//
		//====================================================================================================
		//
		public abstract string PhysicalInstallPath {get;}
		//
		//====================================================================================================
		//
		public abstract string PhysicalWWWPath {get;}
		//
		//====================================================================================================
		//
		public abstract bool TrapErrors {get;}
		//
		//====================================================================================================
		//
		public abstract string AppPath {get;}
		//
		//====================================================================================================
		//
		public abstract string AppRootPath {get;}
		//
		//====================================================================================================
		//
		public abstract string DomainPrimary {get;}
		//
		//====================================================================================================
		//
		public abstract string Domain {get;}
		//
		//====================================================================================================
		//
		public abstract string DomainList {get;}
		//
		//====================================================================================================
		//
		public abstract string FilePath {get;}
		//
		//====================================================================================================
		//
		public abstract string PageDefault {get;}
		//
		//====================================================================================================
		//
		public abstract string VirtualPath {get;}
		//
		//====================================================================================================
		//
		public abstract string EncodeAppRootPath(string Link);
		//
		//====================================================================================================
		//
		public abstract bool IsTesting();
		//
		//====================================================================================================
		//
		public abstract void LogActivity(string Message, int UserID, int OrganizationID);
		//
		//====================================================================================================
		//
		public abstract void ErrorReport(string Message);
		//
		//====================================================================================================
		//
		public abstract void ErrorReport(System.Exception Ex, string Message = "");
		//
		//====================================================================================================
		//
		// 20151121 - not needed, removed to resolve compile issue with com compatibility
		//Public MustOverride Sub ErrorReport(ByVal Err As Microsoft.VisualBasic.ErrObject, Optional ByVal Message As String = "")
		//
		//====================================================================================================
		//
		public abstract void RequestTask(string command, string SQL, string ExportName, string Filename);
		//
		//====================================================================================================
		//
		public abstract void TestPoint(string Message);
		//
		//====================================================================================================
		//
		public abstract int LandingPageId(string DomainName = "");
		//
		//====================================================================================================
		//
		public abstract void LogWarning(string name, string description, string typeOfWarningKey, string instanceKey);
		//
		//====================================================================================================
		//
		public abstract void LogAlarm(string cause);
		//
		//====================================================================================================
		//
		public abstract void addLinkAlias(string linkAlias, int pageId, string queryStringSuffix = "");
		//
		//====================================================================================================
		//
		public abstract string ThrowEvent(string eventNameIdOrGuid);
		//
		//====================================================================================================
		/// <summary>
		/// Returns a list of routes used to configure endpoints, always in Unix slash format without a leading slash
		/// </summary>
		/// <returns></returns>
		public abstract Dictionary<string, routeClass> getRouteDictionary();
		//
		//====================================================================================================
		//
		public class routeClass
		{
			public string virtualRoute;
			public string physicalRoute;
			public routeTypeEnum routeType;
			public int remoteMethodAddonId;
			public int linkAliasId;
			public int linkForwardId;
		}
		//
		public enum routeTypeEnum
		{
			admin,
			remoteMethod,
			linkAlias,
			linkForward
		}

	}

}
