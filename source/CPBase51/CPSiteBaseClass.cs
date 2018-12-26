
using System;

namespace Contensive.BaseClasses {
    //
    //====================================================================================================
    /// <summary>
    /// Application settings and methods
    /// </summary>
    public abstract class CPSiteBaseClass {
        //
        //====================================================================================================
        //
        public abstract string Name { get; }
        //
        //====================================================================================================
        //
        public abstract void SetProperty(string key, string value);
        //
        //====================================================================================================
        //
        public abstract string GetProperty(string key, string value = "");
        //
        //====================================================================================================
        //
        public abstract string GetText(string key, string value = "");
        //
        //====================================================================================================
        //
        public abstract bool GetBoolean(string key, string defaultValue = "");
        public abstract bool GetBoolean(string key, bool defaultValue);
        //
        //====================================================================================================
        //
        public abstract DateTime GetDate(string key, string defaultValue = "");
        public abstract DateTime GetDate(string key, DateTime defaultValue);
        //
        //====================================================================================================
        //
        public abstract int GetInteger(string key, string defaultValue = "");
        public abstract int GetInteger(string key, int defaultValue);
        //
        //====================================================================================================
        //
        public abstract double GetNumber(string key, string defaultValue = "");
        public abstract double GetNumber(string key, double defaultValue);
        //
        //====================================================================================================
        //
        public abstract bool MultiDomainMode { get; }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
        public abstract string PhysicalFilePath { get; }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
        public abstract string PhysicalInstallPath { get; }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
        public abstract string PhysicalWWWPath { get; }
        //
        //====================================================================================================
        //
        public abstract bool TrapErrors { get; }
        //
        //====================================================================================================
        //
        public abstract string AppPath { get; }
        //
        //====================================================================================================
        //
        public abstract string AppRootPath { get; }
        //
        //====================================================================================================
        //
        public abstract string DomainPrimary { get; }
        //
        //====================================================================================================
        //
        public abstract string Domain { get; }
        //
        //====================================================================================================
        //
        public abstract string DomainList { get; }
        //
        //====================================================================================================
        //
        public abstract string FilePath { get; }
        //
        //====================================================================================================
        //
        public abstract string PageDefault { get; }
        //
        //====================================================================================================
        //
        public abstract string VirtualPath { get; }
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
        public abstract void LogActivity(string message, int userID, int organizationId);
        //
        //====================================================================================================
        //
        public abstract void ErrorReport(string message);
        //
        //====================================================================================================
        //
        public abstract void ErrorReport(System.Exception Ex, string message = "");
        //
        //====================================================================================================
        //
        // 20151121 - not needed, removed to resolve compile issue with com compatibility
        //Public MustOverride Sub ErrorReport(ByVal Err As Microsoft.VisualBasic.ErrObject, Optional ByVal Message As String = "")
        //
        //====================================================================================================
        /// <summary>
        /// Run an SQL query on the default datasource and save the data in a CSV file in the filename provided to a record in the tasks table.
        /// </summary>
        public abstract void RequestTask(string command, string SQL, string ExportName, string Filename);
        //
        //====================================================================================================
        //
        public abstract void TestPoint(string message);
        //
        //====================================================================================================
        //
        public abstract int LandingPageId(string domainName = "");
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
        /// 
        /// </summary>
        /// <param name="privatePathFilename"></param>
        /// <param name="returnUserError"></param>
        /// <returns></returns>
        public abstract bool installCollectionFile(string privatePathFilename, ref string returnUserError);
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionGuid"></param>
        /// <param name="returnUserError"></param>
        /// <returns></returns>
        public abstract bool installCollectionFromLibrary(string collectionGuid, ref string returnUserError);
        //
        //====================================================================================================
        // deprecated
        //
    }
}
