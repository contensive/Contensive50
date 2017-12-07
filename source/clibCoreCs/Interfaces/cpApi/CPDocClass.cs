using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Controllers;

using System.Runtime.InteropServices;

namespace Contensive.Core {
    //
    // comVisible to be activeScript compatible
    //
    [ComVisible(true), Microsoft.VisualBasic.ComClass(CPDocClass.ClassId, CPDocClass.InterfaceId, CPDocClass.EventsId)]
    public class CPDocClass : BaseClasses.CPDocBaseClass, IDisposable {
        #region COM GUIDs
        public const string ClassId = "414BD6A9-195F-4E0F-AE24-B7BF56749CDD";
        public const string InterfaceId = "347D06BC-4D68-4DBE-82FE-B72115E24A56";
        public const string EventsId = "95E8786B-E778-4617-96BA-B45C53E4AFD1";
        #endregion
        //
        private CPClass cp;
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpParent"></param>
        public CPDocClass(CPClass cpParent) : base() {
            cp = cpParent;
            cpCore = cp.core;
        }
        //
        //====================================================================================================
        /// <summary>
        /// destructor
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cpCore = null;
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the page content
        /// </summary>
        /// <returns></returns>
        public override string content {
            get {
                if (true) {
                    return cpCore.doc.bodyContent;
                } else {
                    return "";
                }
            }
            set {
                if (true) {
                    cpCore.doc.bodyContent = value;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// deprecated.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use addon navigation.", true)]
        public override string navigationStructure {
            get {
                if (true) {
                    return "";
                } else {
                    return "";
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns to the current value of NoFollow, set by addon execution
        /// </summary>
        /// <returns></returns>
        public override bool noFollow {
            get {
                if (true) {
                    return cpCore.webServer.response_NoFollow;
                } else {
                    return false;
                }
            }
            set {
                if (true) {
                    cpCore.webServer.response_NoFollow = value;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the pageId
        /// </summary>
        /// <returns></returns>
        public override int pageId {
            get {
                if (cpCore.doc.page == null) {
                    return 0;
                } else {
                    return cpCore.doc.page.id;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the page name, set by the pagemenager addon
        /// </summary>
        /// <returns></returns>
        public override string pageName {
            get {
                if (cpCore.doc.page == null) {
                    return "";
                } else {
                    return cpCore.doc.page.name;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the current value of refreshquerystring 
        /// </summary>
        /// <returns></returns>
        public override string refreshQueryString {
            get {
                return cpCore.doc.refreshQueryString;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the value of sectionId
        /// </summary>

        [Obsolete("Section is no longer supported", true)]
        public override int sectionId {
            get {
                return 0;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// the time and date when this document was started 
        /// </summary>
        /// <returns></returns>
        public override DateTime startTime {
            get {
                if (true) {
                    return cpCore.doc.profileStartTime;
                } else {
                    return new DateTime();
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the id of the template, as set by the page manager
        /// </summary>
        /// <returns></returns>
        public override int templateId {
            get {
                if (cpCore.doc != null) {
                    if (cpCore.doc.template != null) {
                        return cpCore.doc.template.ID;
                    }
                }
                return 0;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the docType, set by the page manager settings 
        /// </summary>
        /// <returns></returns>
        public override string Type {
            get {
                return cpCore.siteProperties.docTypeDeclaration;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// adds javascript code to the head of the document
        /// </summary>
        /// <param name="NewCode"></param>
        public override void AddHeadJavascript(string NewCode) {
            cpCore.html.addScriptCode_head(NewCode, "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// adds a javascript tag to the head of the document
        /// </summary>
        /// <param name="HeadTag"></param>
        public override void addHeadTag(string HeadTag) {
            if (true) {
                cpCore.html.addHeadTag(HeadTag);
            }
        }
        //
        //====================================================================================================
        //
        public override void addMetaDescription(string MetaDescription) {
            if (true) {
                cpCore.html.addMetaDescription(MetaDescription);
            }
        }
        //
        //====================================================================================================
        //
        public override void addMetaKeywordList(string MetaKeywordList) {
            if (true) {
                cpCore.html.addMetaKeywordList(MetaKeywordList);
            }
        }
        //
        //====================================================================================================
        //
        public override void addOnLoadJavascript(string NewCode) {
            if (true) {
                cpCore.html.addScriptCode_onLoad(NewCode, "");
            }
        }
        //
        //====================================================================================================
        //
        public override void addTitle(string PageTitle) {
            if (true) {
                cpCore.html.addTitle(PageTitle);
            }
        }
        //
        //====================================================================================================
        //
        public override void addRefreshQueryString(string Name, string Value) {
            if (true) {
                cpCore.doc.addRefreshQueryString(Name, Value);
            }
        }
        //
        //====================================================================================================
        //
        public override void addHeadStyle(string StyleSheet) {
            if (true) {
                addHeadTag(Environment.NewLine + "\t" + "<style type=\"text/css\">" + Environment.NewLine + "\t" + "\t" + StyleSheet + Environment.NewLine + "\t" + "</style>");
            }
        }
        //
        //====================================================================================================
        //
        public override void addHeadStyleLink(string StyleSheetLink) {
            cpCore.html.addStyleLink(StyleSheetLink, "");
        }
        //
        //====================================================================================================
        //
        public override void addBodyEnd(string NewCode) {
            if (true) {
                cpCore.doc.htmlForEndOfBody += NewCode;
            }
        }
        //
        //====================================================================================================
        //
        public override string body {
            get {
                string tempbody = null;
                if (true) {
                    tempbody = cpCore.doc.docBodyFilter;
                } else {
                    return "";
                }
                return tempbody;
            }
            set {
                if (true) {
                    cpCore.doc.docBodyFilter = value;
                }
            }
        }

        //
        //====================================================================================================
        //
        [Obsolete("Site styles are no longer supported. Include styles and javascript in addons.", true)]
        public override string siteStylesheet {
            get {
                return ""; //cpCore.html.html_getStyleSheet2(0, 0)
            }
        }
        //
        //====================================================================================================
        //   Decodes an argument parsed from an AddonOptionString for all non-allowed characters
        //       AddonOptionString is a & delimited string of name=value[selector]descriptor
        //
        //       to get a value from an AddonOptionString, first use getargument() to get the correct value[selector]descriptor
        //       then remove everything to the right of any '['
        //
        //       call encodeaddonoptionargument before parsing them together
        //       call decodeAddonOptionArgument after parsing them apart
        //
        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        //
        //       This routine is needed for all Arg, Name, Value, Option values
        //
        //------------------------------------------------------------------------------------------------------------
        //
        public string decodeLegacyOptionStringArgument(string EncodedArg) {
            string tempdecodeLegacyOptionStringArgument = null;
            string a = null;
            //
            tempdecodeLegacyOptionStringArgument = "";
            if (!string.IsNullOrEmpty(EncodedArg)) {
                a = EncodedArg;
                a = genericController.vbReplace(a, "#0058#", ":");
                a = genericController.vbReplace(a, "#0093#", "]");
                a = genericController.vbReplace(a, "#0091#", "[");
                a = genericController.vbReplace(a, "#0124#", "|");
                a = genericController.vbReplace(a, "#0039#", "'");
                a = genericController.vbReplace(a, "#0034#", "\"");
                a = genericController.vbReplace(a, "#0044#", ",");
                a = genericController.vbReplace(a, "#0061#", "=");
                a = genericController.vbReplace(a, "#0038#", "&");
                a = genericController.vbReplace(a, "#0013#", Environment.NewLine);
                tempdecodeLegacyOptionStringArgument = a;
            }
            return tempdecodeLegacyOptionStringArgument;
        }
        //
        //=======================================================================================================
        //
        public override string GetProperty(string PropertyName, string DefaultValue = "") {
            if (cpCore.docProperties.containsKey(PropertyName)) {
                return cpCore.docProperties.getText(PropertyName);
            } else {
                return DefaultValue;
            }
        }
        //
        //=======================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override double GetNumber(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override string GetText(string FieldName, string DefaultValue = "") {
            return GetProperty(FieldName, DefaultValue);
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override bool IsProperty(string FieldName) {
            return cpCore.docProperties.containsKey(FieldName);
        }
        //
        //=======================================================================================================
        //
        public override void SetProperty(string FieldName, string FieldValue) {
            cpCore.docProperties.setProperty(FieldName, FieldValue);
        }
        //'
        //'=======================================================================================================
        //' Deprecated ------------ us GetProperty and Setproperty
        //' IsVar
        //'   like GlobalVar, but it includes the OptionString
        //'   a set Var adds an etry to LocalVars
        //'=======================================================================================================
        //'
        //Public Overrides Property var(ByVal Name As String) As String
        //    Get
        //        Return cpCore.docProperties.getText(Name)
        //        'Dim lcName As String
        //        'If Name = "" Then
        //        '    var = ""
        //        'Else
        //        '    lcName = Name.ToLower
        //        '    If LocalVars.Contains(lcName) Then
        //        '        var = LocalVars(lcName)
        //        '    Else
        //        '        var = globalVar(lcName)
        //        '    End If
        //        'End If
        //        'Call tp("Property var get exit, " & Name & "=" & var)
        //    End Get
        //    Set(ByVal value As String)
        //        cpCore.docProperties.setProperty(Name, value)
        //        'Dim lcName As String
        //        ''Dim valueObj As String = ""
        //        ''If Not value Is Nothing Then
        //        ''    valueObj = value
        //        ''End If
        //        'value = genericController.encodeText(value)
        //        'Call appendDebugLog("var set, " & Name & "=" & value.ToString)
        //        'If Name <> "" Then
        //        '    lcName = Name.ToLower
        //        '    If LocalVars.Contains(lcName) Then
        //        '        Call LocalVars.Remove(lcName)
        //        '        If value = "" Then
        //        '            LocalVarNameList = genericController.vbReplace(LocalVarNameList, vbCrLf & lcName, "", , , CompareMethod.Text)
        //        '        Else
        //        '            Call LocalVars.Add(value, lcName)
        //        '            Call tp("Property var set, name found in localVars, removed and re-added, LocalVarNameList=" & LocalVarNameList)
        //        '        End If
        //        '    ElseIf value <> "" Then
        //        '        Call LocalVars.Add(value, lcName)
        //        '        LocalVarNameList = LocalVarNameList & vbCrLf & lcName
        //        '        Call tp("Property var set, name not found in localVars so it was added, LocalVarNameList=" & LocalVarNameList)
        //        '    End If
        //        'End If
        //    End Set
        //End Property
        //'
        //'=======================================================================================================
        //' Deprecated ------------ us GetProperty and Setproperty
        //' GlobalVar
        //'   Like ViewingProperties but includes stream
        //'   returns
        //'       matches to ViewingProperties
        //'       then matches to Stream
        //'=======================================================================================================
        //'
        //Public Overrides Property globalVar(ByVal Name As String) As String
        //    Get
        //        Return cpCore.docProperties.getText(Name)
        //        'Dim lcName As String
        //        'If True Then
        //        '    If Name = "" Then
        //        '        globalVar = ""
        //        '    Else
        //        '        lcName = Name.ToLower
        //        '        If cpCore.main_IsViewingProperty(lcName) Then
        //        '            globalVar = cpCore.docProperties.getText(lcName)
        //        '        Else
        //        '            globalVar = cpCore.docProperties.getText(lcName)
        //        '        End If
        //        '    End If
        //        'Else
        //        '    globalVar = ""
        //        'End If
        //    End Get
        //    Set(ByVal value As String)
        //        cpCore.docProperties.setProperty(Name, value)
        //        'If True Then
        //        '    Call cpCore.docProperties.setProperty(Name.ToLower, value)
        //        'End If
        //    End Set
        //End Property
        //'
        //'=======================================================================================================
        //' Deprecated ------------ us GetProperty and Setproperty
        //' IsGlobalVar 
        //'   returns true if
        //'       IsViewingProperties is true or InStream is true
        //'=======================================================================================================
        //'
        //Public Overrides ReadOnly Property isGlobalVar(ByVal Name As String) As Boolean
        //    Get
        //        Return False
        //        'Dim lcName As String
        //        'If True Then
        //        '    If Name = "" Then
        //        '        isGlobalVar = ""
        //        '    Else
        //        '        lcName = Name.ToLower
        //        '        isGlobalVar = cpCore.main_IsViewingProperty(lcName)
        //        '        If Not isGlobalVar Then
        //        '            isGlobalVar = cpCore.docProperties.containsKey(lcName)
        //        '        End If
        //        '    End If
        //        'Else
        //        '    isGlobalVar = False
        //        'End If
        //    End Get
        //End Property
        //'
        //'=======================================================================================================
        //' Deprecated ------------ us GetProperty and Setproperty
        //' IsVar
        //'   returns true if
        //'       matches to GlobalVars or LocalVars collections
        //'=======================================================================================================
        //'
        //Public Overrides ReadOnly Property isVar(ByVal Name As String) As Boolean
        //    Get
        //        Return cpCore.docProperties.containsKey(Name)
        //        'Dim lcName As String
        //        'If Name = "" Then
        //        '    isVar = False
        //        'Else
        //        '    lcName = Name.ToLower
        //        '    isVar = LocalVars.Contains(lcName)
        //        '    If Not isVar Then
        //        '        isVar = isGlobalVar(lcName)
        //        '    End If
        //        'End If
        //    End Get
        //End Property
        //
        //
        //
        public override bool IsAdminSite {
            get {
                bool returnIsAdmin = false;
                try {
                    returnIsAdmin = (cp.Request.PathPage.IndexOf(cp.Site.GetText("adminUrl"), System.StringComparison.OrdinalIgnoreCase) + 1 != 0);
                } catch (Exception ex) {
                    cpCore.handleException(ex); // "unexpected error in IsAdminSite")
                    throw;
                }
                return returnIsAdmin;
            }
        }
        //
        //=======================================================================================================
        //
        // debugging -- append to logfile
        //
        //=======================================================================================================
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDocDebug.log", Now & " - cp.doc, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        //=======================================================================================================
        //
        // debugging -- testpoint
        //
        //=======================================================================================================
        //
        private void tp(string msg) {
            //Call appendDebugLog(msg)
        }
        //
        //=======================================================================================================
        //
        // IDisposable support
        //
        //=======================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPDocClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}