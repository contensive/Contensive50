
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Controllers {
    public class dataTreeController {
        //========================================================================
        // This page and its contents are copyright by Kidwell McGowan Associates.
        //========================================================================
        //
        // ----- global scope variables
        //
        private coreClass cpCore;
        private bool ClassInitialized; // if true, the module has been
        private XmlDocument MSxml;
        //
        // ----- Tracking values, should be set before each exit
        //
        //Private NodePtr as integer
        private struct TierNode {
            public XmlNode Node;
            //    Element As xmlNode
            public int ChildPtr;
            public bool ChildPtrOK;
            public int AttrPtr;
        }
        //
        // Track node position
        //   TierPtr = 0 is above the Root
        //   TierPtr = 1 is the Root Node
        //   Tier 2... are child tiers
        //
        private int TierPtr;
        private TierNode[] Tier;
        //
        //
        //
        private bool Private_IsEmpty;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpCore"></param>
        /// <remarks></remarks>
        public dataTreeController(coreClass cpCore) : base() {
            this.cpCore = cpCore;
            ClassInitialized = true;
        }
        //
        //
        //
        private bool Load(string XMLSource, int LoadType) {
            bool tempLoad = false;
            //On Error GoTo ErrorTrap
            //
            int Ptr = 0;
            int ChildPtr = 0;
            //
            // Clear Globals
            //
            Private_IsEmpty = true;
            tempLoad = false;
            Tier = new Contensive.Core.Controllers.dataTreeController.TierNode[1];
            TierPtr = 0;
            Tier[0].Node = null;
            //Set Tier(0).Element = Nothing
            Tier[0].ChildPtr = 0;
            Tier[0].ChildPtrOK = false;
            Tier[0].AttrPtr = 0;
            //
            // Load new msxml
            //
            MSxml = new XmlDocument();
            if (!string.IsNullOrEmpty(XMLSource)) {
                bool loadOK = true;
                try {
                    switch (LoadType) {
                        case 1:
                            MSxml.Load(XMLSource);
                            break;
                        default:
                            MSxml.LoadXml(XMLSource);
                            break;
                    }
                } catch (Exception ex) {
                    cpCore.handleException(ex);
                    loadOK = false;
                }
                if (loadOK) {
                    tempLoad = true;
                    Private_IsEmpty = false;
                    if (MSxml.ChildNodes.Count != 0) {
                        //
                        // ----- Set Tier(1) to root node
                        //
                        for (Ptr = 0; Ptr < MSxml.ChildNodes.Count; Ptr++) {
                            if (MSxml.ChildNodes[Ptr].NodeType == System.Xml.XmlNodeType.Element) {
                                //
                                Tier[0].ChildPtr = ChildPtr;
                                Tier[0].ChildPtrOK = true;
                                //
                                TierPtr = 1;
                                Array.Resize(ref Tier, 2);
                                Tier[1].Node = MSxml.ChildNodes[Ptr];
                                Tier[1].ChildPtr = 0;
                                Tier[1].ChildPtrOK = true;
                                Tier[1].AttrPtr = 0;
                                break;
                            }
                        }
                    }
                }
            }
            return tempLoad;
        }
        //
        //
        //
        public void LoadURL(string XMLURL) {
            Load(XMLURL, 1);
        }
        //
        //
        //
        public void LoadText(string XMLData) {
            Load(XMLData, 0);
        }
        //
        //
        //
        public void LoadFile(string Filename) {
            Load(cpCore.appRootFiles.readFile(Filename), 0);
        }
        //
        // Get Name
        //
        public string NodeName {
            get {
                string tempNodeName = null;
                tempNodeName = "";
                if (TierPtr > 0) {
                    if (Tier[TierPtr].Node != null) {
                        tempNodeName = Tier[TierPtr].Node.Name;
                    }
                }

                return tempNodeName;
            }
            set {
                if (Private_IsEmpty) {
                    TierPtr = 1;
                    MSxml = new XmlDocument();
                    Tier = new Contensive.Core.Controllers.dataTreeController.TierNode[TierPtr + 1];
                    Tier[TierPtr].Node = MSxml.CreateElement(value);
                    Private_IsEmpty = false;
                    //Call AddChild(vNewValue)
                } else {
                    if (Tier[TierPtr].Node != null) {
                        //Tier(TierPtr).Node.Name = vNewValue
                    }
                }

            }
        }
        //
        // Next Node
        //
        public void GoNext() {
            try {
                int ParentPtr = 0;
                //
                if (TierPtr > 0) {
                    ParentPtr = TierPtr - 1;
                    if (Tier[ParentPtr].ChildPtrOK) {
                        Tier[ParentPtr].ChildPtr = Tier[ParentPtr].ChildPtr + 1;
                        Tier[ParentPtr].ChildPtrOK = (Tier[ParentPtr].Node.ChildNodes.Count > Tier[ParentPtr].ChildPtr);
                        Tier[TierPtr].Node = null;
                        Tier[TierPtr].AttrPtr = 0;
                        if (Tier[ParentPtr].ChildPtrOK) {
                            if (Tier[ParentPtr].Node.ChildNodes[Tier[ParentPtr].ChildPtr].NodeType == System.Xml.XmlNodeType.Element) {
                                Tier[TierPtr].Node = Tier[ParentPtr].Node.ChildNodes[Tier[ParentPtr].ChildPtr];
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        // IsNodeOK
        //
        public bool IsNodeOK() {
            bool tempIsNodeOK = false;
            tempIsNodeOK = false;
            if (TierPtr > 0) {
                tempIsNodeOK = Tier[TierPtr - 1].ChildPtrOK;
            }
            return tempIsNodeOK;
        }
        //
        // Previous Node
        //
        public void GoPrevious() {
            //
            int ParentPtr = 0;
            //
            if (TierPtr > 0) {
                ParentPtr = TierPtr - 1;
                if (Tier[ParentPtr].ChildPtrOK) {
                    Tier[ParentPtr].ChildPtr = Tier[ParentPtr].ChildPtr + 1;
                    Tier[ParentPtr].ChildPtrOK = (Tier[ParentPtr].ChildPtr >= 0);
                    Tier[TierPtr].Node = null;
                    Tier[TierPtr].AttrPtr = 0;
                    if (Tier[ParentPtr].ChildPtrOK) {
                        if (Tier[ParentPtr].Node.ChildNodes[Tier[TierPtr].ChildPtr].NodeType == System.Xml.XmlNodeType.Element) {
                            Tier[TierPtr].Node = Tier[ParentPtr].Node.ChildNodes[Tier[TierPtr].ChildPtr];
                        }
                    }
                }
            }
        }
        //
        //
        //
        public int ChildCount() {
            int tempChildCount = 0;
            //
            tempChildCount = 0;
            if (Tier[TierPtr].Node != null) {
                tempChildCount = Tier[TierPtr].Node.ChildNodes.Count;
            }
            return tempChildCount;
        }
        //
        //
        //
        public void GoFirstChild() {
            try {
                int Ptr = 0;
                //
                if (Tier[TierPtr].Node != null) {
                    Tier[TierPtr].ChildPtr = 0;
                    Tier[TierPtr].ChildPtrOK = false;
                    if (Tier[TierPtr].Node.ChildNodes.Count > 0) {
                        //
                        // setup new tier
                        //
                        TierPtr = TierPtr + 1;
                        Array.Resize(ref Tier, TierPtr + 1);
                        Tier[TierPtr].Node = Tier[TierPtr - 1].Node.ChildNodes[Ptr];
                        Tier[TierPtr].ChildPtr = 0;
                        Tier[TierPtr].ChildPtrOK = (Tier[TierPtr - 1].Node.ChildNodes.Count > 0);
                        Tier[TierPtr].AttrPtr = 0;
                        //
                        // set parent tier ptrs
                        //
                        Tier[TierPtr - 1].ChildPtr = 0;
                        Tier[TierPtr - 1].ChildPtrOK = true;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }

        //
        //
        //
        public void GoParent() {
            TierPtr = TierPtr - 1;
        }
        //
        //
        //
        public string GetAttrNames() {
            string tempGetAttrNames = null;
            //
            //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
            //			XmlAttribute NodeAttribute = null;
            //
            tempGetAttrNames = "";
            if (TierPtr > 0) {
                foreach (XmlAttribute NodeAttribute in Tier[TierPtr].Node.Attributes) {
                    tempGetAttrNames = tempGetAttrNames + "\r\n" + NodeAttribute.Name;
                    //End If
                }
            }
            if (!string.IsNullOrEmpty(tempGetAttrNames)) {
                tempGetAttrNames = tempGetAttrNames.Substring(2);
            }
            return tempGetAttrNames;
        }
        //
        //
        //
        public void AddChild(string NodeName) {
            //
            XmlNode ChildNode = null;
            //
            if (Private_IsEmpty) {
                NodeName = "RootNode";
            }
            if (TierPtr > 0) {
                ChildNode = MSxml.CreateNode(System.Xml.XmlNodeType.Element, NodeName, "");
                Tier[TierPtr].Node.AppendChild(ChildNode);
                Private_IsEmpty = false;
            }
        }
        //
        //
        //
        public void AddAttr(string AttrName, string AttrValue) {
            //
            //    MSxml.documentElement
            //
            //    Dim DocElement As xmlNode
            //    Dim ChildNode As xmlnode
            //    Dim Attr As xmlattribute
            //    '
            //    If TierPtr > 0 Then
            //        Attr = MSxml.createAttribute(AttrName)
            //        Tier(TierPtr).Node.seta
            //    End If
        }
        //
        //
        //
        public int AttrCount() {
            int tempAttrCount = 0;
            //
            tempAttrCount = 0;
            if (Tier[TierPtr].Node != null) {
                if (Tier[TierPtr].Node.Attributes != null) {
                    tempAttrCount = Tier[TierPtr].Node.Attributes.Count;
                }
            }
            return tempAttrCount;
        }
        //
        //
        //
        public void GoPreviousAttr() {
            //
            if (IsAttrOK()) {
                Tier[TierPtr].AttrPtr = Tier[TierPtr].AttrPtr - 1;
            }
        }
        //
        //
        //
        public void GoNextAttr() {
            //
            if (IsAttrOK()) {
                Tier[TierPtr].AttrPtr = Tier[TierPtr].AttrPtr + 1;
            }
        }
        //
        // IsAttrOK
        //
        public bool IsAttrOK() {
            bool tempIsAttrOK = false;
            tempIsAttrOK = false;
            if (TierPtr > 0) {
                if (Tier[TierPtr].Node != null) {
                    tempIsAttrOK = (Tier[TierPtr].AttrPtr >= 0) && (Tier[TierPtr].AttrPtr < Tier[TierPtr].Node.Attributes.Count);
                }
            }
            return tempIsAttrOK;
        }
        //
        //
        //
        public string GetAttr(string AttrName) {
            string tempGetAttr = null;
            tempGetAttr = "";
            //
            int AttrPtr = 0;
            int AttrCnt = 0;
            string UCaseAttrName = null;
            //
            AttrCnt = Tier[TierPtr].Node.Attributes.Count;
            if (AttrCnt > 0 && (!string.IsNullOrEmpty(AttrName))) {
                UCaseAttrName = genericController.vbUCase(AttrName);
                for (AttrPtr = 0; AttrPtr < AttrCnt; AttrPtr++) {
                    if (UCaseAttrName == genericController.vbUCase(Tier[TierPtr].Node.Attributes[AttrPtr].Name)) {
                        tempGetAttr = Tier[TierPtr].Node.Attributes[AttrPtr].Value;
                        break;
                    }
                }
            }
            return tempGetAttr;
        }
        //
        //
        //
        public string GetAttrName() {
            string tempGetAttrName = null;
            int AttrPtr = 0;
            tempGetAttrName = "";
            if (IsAttrOK()) {
                AttrPtr = Tier[TierPtr].AttrPtr;
                tempGetAttrName = Tier[TierPtr].Node.Attributes[AttrPtr].Name;
            }
            return tempGetAttrName;
        }
        //
        //
        //
        public string GetAttrValue() {
            string tempGetAttrValue = null;
            int AttrPtr = 0;
            tempGetAttrValue = "";
            if (IsAttrOK()) {
                AttrPtr = Tier[TierPtr].AttrPtr;
                tempGetAttrValue = Tier[TierPtr].Node.Attributes[AttrPtr].Value;
            }
            return tempGetAttrValue;
        }
        //
        public string XML {
            get {
                string tempXML = null;
                tempXML = "";
                if (Tier[TierPtr].Node != null) {
                    tempXML = Tier[TierPtr].Node.InnerXml;
                }
                return tempXML;
            }
        }
        //
        //
        //
        public string Text {
            get {
                string tempText = null;
                tempText = "";
                if (Tier[TierPtr].Node != null) {
                    tempText = Tier[TierPtr].Node.InnerText;
                }
                return tempText;
            }
        }
        //
        //
        //
        public void Clear() {
            Private_IsEmpty = true;
            if (TierPtr <= 1) {
                MSxml = null;
            } else {
                if (Tier[TierPtr].Node != null) {
                    Tier[TierPtr - 1].Node.RemoveChild(Tier[TierPtr].Node);
                }
            }
        }
        //
        //
        //
        public void SaveFile(string Filename) {
            cpCore.appRootFiles.saveFile(Filename, XML);
        }
        //
        //
        //


        public bool IsEmpty {
            get {
                return Private_IsEmpty;
            }
        }
    }
}

