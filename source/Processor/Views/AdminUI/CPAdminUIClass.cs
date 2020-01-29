
using Contensive.BaseClasses.AdminUI;
using Contensive.BaseModels;
using Contensive.Processor.AdminUI;
using System;
using System.Collections.Generic;

namespace Contensive.Processor {
    public class CPAdminUIClass : BaseClasses.CPAdminUIBaseClass {
        //
        public CPAdminUIClass(Controllers.CoreController core) {
            this.core = core;
        }
        //
        private readonly Contensive.Processor.Controllers.CoreController core;
        //
        // ====================================================================================================
        //
        public override string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIEditorController.getBooleanEditor(core, htmlName, htmlValue, readOnly, htmlId, required);

        public override string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIEditorController.getBooleanEditor(core, htmlName, htmlValue, readOnly, htmlId);

        public override string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId)
            => Controllers.AdminUIEditorController.getBooleanEditor(core, htmlName, htmlValue, false, htmlId, false);

        public override string GetBooleanEditor(string htmlName, bool htmlValue)
            => Controllers.AdminUIEditorController.getBooleanEditor(core, htmlName, htmlValue, false, "", false);

        public override string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIEditorController.getDateTimeEditor(core, htmlName, htmlValue, readOnly, htmlId, required, "");

        public override string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIEditorController.getDateTimeEditor(core, htmlName, htmlValue, readOnly, htmlId, false, "");

        public override string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId)
            => Controllers.AdminUIEditorController.getDateTimeEditor(core, htmlName, htmlValue, false, htmlId, false, "");

        public override string GetDateTimeEditor(string htmlName, DateTime? htmlValue)
            => Controllers.AdminUIEditorController.getDateTimeEditor(core, htmlName, htmlValue, false, "", false, "");

        public override string GetEditRow(string caption, string editor)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, "");

        public override string GetEditRow(string caption, string editor, string help)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help);

        public override string GetEditRow(string caption, string editor, string help, string htmlId)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help, false, false, htmlId);

        public override string GetEditRow(string caption, string editor, string help, string htmlId, bool required)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help, required, false, htmlId);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, readOnly, htmlId, required);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, readOnly, htmlId, false);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId)
            => Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, false, htmlId, false);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue)
            => Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, false, "");

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIEditorController.getHtmlEditor(core, htmlName, htmlValue, "", "", "", readOnly, htmlId);

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIEditorController.getHtmlEditor(core, htmlName, htmlValue, "", "", "", readOnly, htmlId);

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId)
            => Controllers.AdminUIEditorController.getHtmlEditor(core, htmlName, htmlValue, "", "", "", false);

        public override string GetHtmlEditor(string htmlName, string htmlValue)
            => Controllers.AdminUIEditorController.getHtmlEditor(core, htmlName, htmlValue, "", "", "", false, "");

        public override string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIEditorController.getIntegerEditor(core, htmlName, htmlValue, readOnly, htmlId, required,"");

        public override string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIEditorController.getIntegerEditor(core, htmlName, htmlValue, readOnly, htmlId, false, "");

        public override string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId)
            => Controllers.AdminUIEditorController.getIntegerEditor(core, htmlName, htmlValue, false, htmlId, false, "");

        public override string GetIntegerEditor(string htmlName, int? htmlValue)
            => Controllers.AdminUIEditorController.getIntegerEditor(core, htmlName, htmlValue, false, "", false, "");

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly, bool required, string sqlFilter) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", required, sqlFilter);
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly, bool required) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", required, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, false, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, false, "", "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly, bool required, string sqlFilter) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", required, sqlFilter);
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly, bool required) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", required, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, false, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName) {
            bool isEmptyList = false;
            return Controllers.AdminUIEditorController.getLookupContentEditor(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, false, "", "", false, "");
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupName, lookupList, readOnly, htmlId, "", required);
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupName, lookupList, readOnly, htmlId, "", false);
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupName, lookupList, false, htmlId, "", false);
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupName, lookupList, false, "", "", false);
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupValue, lookupList, readOnly, htmlId, "", required);
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupValue, lookupList, readOnly, htmlId, "", false);
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupValue, lookupList, false, htmlId, "", false);
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList) {
            return Controllers.AdminUIEditorController.getLookupListEditor(core, htmlName, currentLookupValue, lookupList, false, "", "", false);
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupGuid, "", readOnly, htmlId, required, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupGuid, "", readOnly, htmlId, false, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupGuid, "", false, htmlId, false, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupGuid, "", false, "", false, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupId, "", readOnly, htmlId, required, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupId, "", readOnly, htmlId, false, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupId, "", false, htmlId, false, "");
        }

        public override string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId) {
            return Controllers.AdminUIEditorController.getMemberSelectEditor(core, htmlName, lookupPersonId, groupId, "", false, "", false, "");
        }

        public override string GetNumberEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getNumberEditor(core, htmlName, htmlValue, readOnly, htmlId, required, "");
        }

        public override string GetNumberEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getNumberEditor(core, htmlName, htmlValue, readOnly, htmlId, false, "");
        }

        public override string GetNumberEditor(string htmlName, double? htmlValue, string htmlId) {
            return Controllers.AdminUIEditorController.getNumberEditor(core, htmlName, htmlValue, false, htmlId, false, "");
        }

        public override string GetNumberEditor(string htmlName, double? htmlValue) {
            return Controllers.AdminUIEditorController.getNumberEditor(core, htmlName, htmlValue, false, "", false, "");
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, readOnly, htmlId, required);
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, readOnly, htmlId, false);
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId) {
            return Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, false, htmlId, false);
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue) {
            return Controllers.AdminUIEditorController.getHtmlCodeEditor(core, htmlName, htmlValue, false, "", false);
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIEditorController.getTextEditor(core, htmlName, htmlValue, readOnly, htmlId, required);
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly) {
            return Controllers.AdminUIEditorController.getTextEditor(core, htmlName, htmlValue, readOnly, htmlId);
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId) {
            return Controllers.AdminUIEditorController.getTextEditor(core, htmlName, htmlValue, false, htmlId);
        }

        public override string GetTextEditor(string htmlName, string htmlValue) {
            return Controllers.AdminUIEditorController.getTextEditor(core, htmlName, htmlValue);
        }

        public override ToolFormBaseClass NewToolForm() {
            return new CPAdminUIToolFormClass(core);
        }
    }
}