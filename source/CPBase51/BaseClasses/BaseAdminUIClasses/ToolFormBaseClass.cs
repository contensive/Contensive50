
using System;
using Contensive.BaseClasses;

namespace Contensive.BaseClasses.AdminUI {
    public abstract class ToolFormBaseClass {
        //
        //-------------------------------------------------
        //
        public abstract bool IncludeBodyPadding { get; set; }
        //
        //-------------------------------------------------
        //
        public abstract bool IncludeBodyColor { get; set; }
        //
        //-------------------------------------------------
        //
        public abstract bool IsOuterContainer { get; set; }
        //
        //-------------------------------------------------
        // 
        public abstract string Title { get; set; }
        //
        //-------------------------------------------------
        // 
        public abstract string Warning { get; set; }
        //
        //-------------------------------------------------
        // 
        public abstract string Description { get; set; }
        // 
        //-------------------------------------------------
        //
        public abstract string GetHtml(CPBaseClass cp);
        //
        //-------------------------------------------------
        // 
        public abstract void AddFormHidden(string Name, string Value);
        //
        public abstract void AddFormHidden(string name, int value);
        //
        public abstract void AddFormHidden(string name, double value);
        //
        public abstract void AddFormHidden(string name, DateTime value);
        //
        public abstract void AddFormHidden(string name, bool value);
        //
        //-------------------------------------------------
        // 
        public abstract void AddFormButton(string buttonValue);
        public abstract void AddFormButton(string buttonValue, string buttonName);
        public abstract void AddFormButton(string buttonValue, string buttonName, string buttonId);
        public abstract void AddFormButton(string buttonValue, string buttonName, string buttonId, string buttonClass);
        //
        //-------------------------------------------------
        // 
        public abstract string FormActionQueryString { get; set; }
        public abstract string FormId { get; set; }
        //
        //-------------------------------------------------
        // 
        public abstract string Body { get; set; }
    }
}
