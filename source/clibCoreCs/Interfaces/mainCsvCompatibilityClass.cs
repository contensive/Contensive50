

using System.Runtime.InteropServices;
// 

namespace Contensive.Core {
    
    // '' <summary>
    // '' This class provides a compatibility api for legacy active-script addons that use the "cclib" object for main class nad "csv" for the csv object
    // '' </summary>
    [ComVisible(true)]
    [ComClass(mainCsvScriptCompatibilityClass.ClassId, mainCsvScriptCompatibilityClass.InterfaceId, mainCsvScriptCompatibilityClass.EventsId)]
    public class mainCsvScriptCompatibilityClass {
        
        public const string ClassId = "D9099AAE-3FCB-4398-B94C-19EE7FA97B2B";
        
        public const string InterfaceId = "CE342EA5-339F-4C31-9F90-F878F527E17A";
        
        public const string EventsId = "21D9D0FB-9B5B-43C2-A7A5-3C84ABFAF90A";
        
        // 
        private coreClass cpCore;
        
        public mainCsvScriptCompatibilityClass(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // 
        public string EncodeContent9(
                    object Source, 
                    int i0, 
                    string s2, 
                    int i3, 
                    int i4, 
                    bool b5, 
                    bool b6, 
                    bool b7, 
                    bool b8, 
                    bool b9, 
                    bool b10, 
                    string s11, 
                    string s12, 
                    bool b13, 
                    int i14, 
                    string s15, 
                    int i16) {
            return cpCore.html.convertActiveContentToHtmlForWysiwygEditor(Controllers.genericController.encodeText(Source));
            // Return cpCore.html.convertActiveContent_LegacyInternal(Controllers.genericController.encodeText(Source), 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, False, Nothing, False)
        }
    }
}