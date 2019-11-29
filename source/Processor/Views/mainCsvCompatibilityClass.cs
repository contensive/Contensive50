
using System;
using Contensive.Processor.Controllers;
 
namespace Contensive.Processor {
    public class mainCsvScriptCompatibilityClass {
        public const string ClassId = "D9099AAE-3FCB-4398-B94C-19EE7FA97B2B";
        public const string InterfaceId = "CE342EA5-339F-4C31-9F90-F878F527E17A";
        public const string EventsId = "21D9D0FB-9B5B-43C2-A7A5-3C84ABFAF90A";
        // 
        private readonly CoreController core;
        public mainCsvScriptCompatibilityClass(CoreController core) {
            this.core = core;
        }
        //
        public void SetViewingProperty( string propertyName , string propertyValue ) {
            core.siteProperties.setProperty(propertyName, propertyValue);
        }
        //
        public string EncodeContent9(string Source, int personalizationPeopleId , string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool PlainText, bool AddLinkEID, bool EncodeActiveFormatting , bool EncodeActiveImages , bool EncodeActiveEditIcons , bool EncodeActivePersonalization, string AddAnchorQuery , string ProtocolHostString , bool IsEmailContent ,  int DefaultWrapperID , String ignore_TemplateCaseOnly_Content, int addonContext ) {
            return ActiveContentController.encode(core, Source, personalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, PlainText, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, AddAnchorQuery, ProtocolHostString, IsEmailContent, DefaultWrapperID, ignore_TemplateCaseOnly_Content, (CPUtilsClass.addonContext)addonContext, core.session.isAuthenticated, null, false);
        }
    }
}