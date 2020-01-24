
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
            => Controllers.AdminUIController.getDefaultEditor_bool(core, htmlName, htmlValue, readOnly, htmlId, required);

        public override string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIController.getDefaultEditor_bool(core, htmlName, htmlValue, readOnly, htmlId);

        public override string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId)
            => Controllers.AdminUIController.getDefaultEditor_bool(core, htmlName, htmlValue, false, htmlId, false);

        public override string GetBooleanEditor(string htmlName, bool htmlValue)
            => Controllers.AdminUIController.getDefaultEditor_bool(core, htmlName, htmlValue, false, "", false);

        public override string GetDateTimeEditor(string htmlName, DateTime htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIController.getDefaultEditor_dateTime(core, htmlName, htmlValue, readOnly, htmlId, required, "");

        public override string GetDateTimeEditor(string htmlName, DateTime htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIController.getDefaultEditor_dateTime(core, htmlName, htmlValue, readOnly, htmlId, false, "");

        public override string GetDateTimeEditor(string htmlName, DateTime htmlValue, string htmlId)
            => Controllers.AdminUIController.getDefaultEditor_dateTime(core, htmlName, htmlValue, false, htmlId, false, "");

        public override string GetDateTimeEditor(string htmlName, DateTime htmlValue)
            => Controllers.AdminUIController.getDefaultEditor_dateTime(core, htmlName, htmlValue, false, "", false, "");

        public override string GetEditRow(string caption, string editor)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, "");

        public override string GetEditRow(string caption, string editor, string help)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help);

        public override string GetEditRow(string caption, string editor, string help, string htmlId)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help, false, false, htmlId);

        public override string GetEditRow(string caption, string editor, string help, string htmlId, bool required)
            => Controllers.AdminUIController.getEditRow(core, editor, caption, help, required, false, htmlId);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIController.getDefaultEditor_TextArea(core, htmlName, htmlValue, readOnly, htmlId, required);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIController.getDefaultEditor_TextArea(core, htmlName, htmlValue, readOnly, htmlId, false);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId)
            => Controllers.AdminUIController.getDefaultEditor_TextArea(core, htmlName, htmlValue, false, htmlId, false);

        public override string GetHtmlCodeEditor(string htmlName, string htmlValue)
            => Controllers.AdminUIController.getDefaultEditor_TextArea(core, htmlName, htmlValue, false, "");

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIController.getDefaultEditor_Html(core, htmlName, htmlValue, "", "", "", readOnly, htmlId);

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIController.getDefaultEditor_Html(core, htmlName, htmlValue, "", "", "", readOnly, htmlId);

        public override string GetHtmlEditor(string htmlName, string htmlValue, string htmlId)
            => Controllers.AdminUIController.getDefaultEditor_Html(core, htmlName, htmlValue, "", "", "", false);

        public override string GetHtmlEditor(string htmlName, string htmlValue)
            => Controllers.AdminUIController.getDefaultEditor_Html(core, htmlName, htmlValue, "", "", "", false, "");

        public override string GetIntegerEditor(string htmlName, int htmlValue, string htmlId, bool readOnly, bool required)
            => Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue.ToString(), readOnly, htmlId, required);

        public override string GetIntegerEditor(string htmlName, int htmlValue, string htmlId, bool readOnly)
            => Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue.ToString(), readOnly, htmlId, false);

        public override string GetIntegerEditor(string htmlName, int htmlValue, string htmlId)
            => Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue.ToString(), false, htmlId, false);

        public override string GetIntegerEditor(string htmlName, int htmlValue)
            => Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue.ToString(), false, "", false);

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly, bool required, string sqlFilter) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", required, sqlFilter);
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly, bool required) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", required,"");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId, bool readOnly) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, readOnly, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId, string htmlId) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, false, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, int lookupContentId) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentId, ref isEmptyList, false, "", "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly, bool required, string sqlFilter) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", required, sqlFilter);
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly, bool required) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", required, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId, bool readOnly) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, readOnly, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName, string htmlId) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, false, htmlId, "", false, "");
        }

        public override string GetLookupContentEditor(string htmlName, int htmlValue, string lookupContentName) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupContent(core, htmlName, htmlValue, lookupContentName, ref isEmptyList, false, "", "", false, "");
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId, bool readOnly, bool required) {
            bool isEmptyList = false;
            return Controllers.AdminUIController.getDefaultEditor_lookupList(core, htmlName, currentLookupName, lookupList, readOnly, htmlId, "", required);
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetLookupListEditor(string htmlName, string currentLookupName, List<string> lookupList) {
            throw new NotImplementedException();
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId, bool readOnly, bool required) {
            return Controllers.AdminUIController.getDefaultEditor_lookupList(core, htmlName, currentLookupValue, lookupList, readOnly, htmlId, "", required);
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetLookupListEditor(string htmlName, int currentLookupValue, List<string> lookupList) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, string groupGuid, string htmlId, bool readOnly, bool required) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, string groupGuid, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, string groupGuid, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, string groupGuid) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, int groupId, string htmlId, bool readOnly, bool required) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, int groupId, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, int groupId, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetMemberSelectEditor(string htmlName, int htmlValue, int groupId) {
            throw new NotImplementedException();
        }

        public override string GetNumberEditor(string htmlName, double htmlValue, string htmlId, bool readOnly, bool required) {
            throw new NotImplementedException();
        }

        public override string GetNumberEditor(string htmlName, double htmlValue, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetNumberEditor(string htmlName, double htmlValue, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetNumberEditor(string htmlName, double htmlValue) {
            throw new NotImplementedException();
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required) {
            throw new NotImplementedException();
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId, bool readOnly) {
            throw new NotImplementedException();
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue, string htmlId) {
            throw new NotImplementedException();
        }

        public override string GetTextAreaEditor(string htmlName, string htmlValue) {
            throw new NotImplementedException();
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required) {
            return Contensive.Processor.Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue, readOnly, htmlId);
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly) {
            return Contensive.Processor.Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue, readOnly, htmlId);
        }

        public override string GetTextEditor(string htmlName, string htmlValue, string htmlId) {
            return Contensive.Processor.Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue, false, htmlId);
        }

        public override string GetTextEditor(string htmlName, string htmlValue) {
            return Contensive.Processor.Controllers.AdminUIController.getDefaultEditor_text(core, htmlName, htmlValue);
        }

        public override ToolFormBaseClass NewToolForm() {
            return new CPAdminUIToolFormClass(core);
        }
    }
}