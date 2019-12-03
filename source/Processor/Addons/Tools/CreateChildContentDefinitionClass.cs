
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using System.Text;
using System.Collections.Generic;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class CreateChildContentDefinitionClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// blank return on OK or cancel button
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return GetForm_CreateChildContent(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetForm_CreateChildContent(CoreController core) {
            string result = "";
            try {
                int ParentContentId = 0;
                string ChildContentName = "";
                bool AddAdminMenuEntry = false;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getHeaderTitleDescription("Create a Child Content from a Content Definition", "This tool creates a Content Definition based on another Content Definition."));
                //
                //   print out the submit form
                if (core.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    ParentContentId = core.docProperties.getInteger("ParentContentID");
                    var parentContentMetadata = ContentMetadataModel.create(core, ParentContentId);
                    ChildContentName = core.docProperties.getText("ChildContentName");
                    AddAdminMenuEntry = core.docProperties.getBoolean("AddAdminMenuEntry");
                    //
                    Stream.Add(SpanClassAdminSmall);
                    if ((parentContentMetadata == null) || (string.IsNullOrEmpty(ChildContentName))) {
                        Stream.Add("<p>You must select a parent and provide a child name.</p>");
                    } else {
                        //
                        // Create Definition
                        //
                        Stream.Add("<P>Creating content [" + ChildContentName + "] from [" + parentContentMetadata + "]");
                        var childContentMetadata = parentContentMetadata.createContentChild(core, ChildContentName, core.session.user.id);
                        //
                        Stream.Add("<br>Reloading Content Definitions...");
                        core.cache.invalidateAll();
                        core.clearMetaData();
                        Stream.Add("<br>Finished</P>");
                    }
                    Stream.Add("</SPAN>");
                }
                Stream.Add(SpanClassAdminNormal);
                //
                Stream.Add("Parent Content Name<br>");
                Stream.Add(core.html.selectFromContent("ParentContentID", ParentContentId, "Content", ""));
                Stream.Add("<br><br>");
                //
                Stream.Add("Child Content Name<br>");
                Stream.Add(HtmlController.inputText_Legacy(core, "ChildContentName", ChildContentName, 1, 40));
                Stream.Add("<br><br>");
                //
                Stream.Add("Add Admin Menu Entry under Parent's Menu Entry<br>");
                Stream.Add(HtmlController.checkbox("AddAdminMenuEntry", AddAdminMenuEntry));
                Stream.Add("<br><br>");
                //
                //Stream.Add( core.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateChildContent)
                Stream.Add("</SPAN>");
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
    }
}

