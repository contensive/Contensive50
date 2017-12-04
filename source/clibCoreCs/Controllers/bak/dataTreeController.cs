


using System.Xml;
// 

namespace Controllers {
    
    public class dataTreeController {
        
        // ========================================================================
        //  This page and its contents are copyright by Kidwell McGowan Associates.
        // ========================================================================
        // 
        //  ----- global scope variables
        // 
        private coreClass cpCore;
        
        private bool ClassInitialized;
        
        //  if true, the module has been
        private XmlDocument MSxml;
        
        // 
        //  ----- Tracking values, should be set before each exit
        // 
        // Private NodePtr as integer
        private struct TierNode {
            
            private XmlNode Node;
            
            //     Element As xmlNode
            private int ChildPtr;
            
            private bool ChildPtrOK;
            
            private int AttrPtr;
        }
        
        // 
        //  Track node position
        //    TierPtr = 0 is above the Root
        //    TierPtr = 1 is the Root Node
        //    Tier 2... are child tiers
        // 
        private int TierPtr;
        
        private TierNode[] Tier;
        
        // 
        // 
        // 
        private bool Private_IsEmpty;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cpCore"></param>
        // '' <remarks></remarks>
        public dataTreeController(coreClass cpCore) {
            this.cpCore = cpCore;
            ClassInitialized = true;
        }
        
        // 
        // 
        // 
        private bool Load(string XMLSource, int LoadType) {
            // On Error GoTo ErrorTrap
            // 
            int Ptr;
            int ChildPtr;
            // 
            //  Clear Globals
            // 
            Private_IsEmpty = true;
            Load = false;
            object Tier;
            TierPtr = 0;
            Tier[0].Node = null;
            Tier[0].ChildPtr = 0;
            Tier[0].ChildPtrOK = false;
            Tier[0].AttrPtr = 0;
            // 
            //  Load new msxml
            // 
            MSxml = new XmlDocument();
            if ((XMLSource != "")) {
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
                }
                catch (Exception ex) {
                    cpCore.handleException(ex);
                    throw;
                }
                
                if (loadOK) {
                    Load = true;
                    Private_IsEmpty = false;
                    if ((MSxml.ChildNodes.Count != 0)) {
                        // 
                        //  ----- Set Tier(1) to root node
                        // 
                        for (Ptr = 0; (Ptr 
                                    <= (MSxml.ChildNodes.Count - 1)); Ptr++) {
                            if ((MSxml.ChildNodes(Ptr).NodeType == System.Xml.XmlNodeType.Element)) {
                                // 
                                Tier[0].ChildPtr = ChildPtr;
                                Tier[0].ChildPtrOK = true;
                                TierPtr = 1;
                                object Preserve;
                                Tier[1];
                                Tier[1].Node = MSxml.ChildNodes(Ptr);
                                Tier[1].ChildPtr = 0;
                                Tier[1].ChildPtrOK = true;
                                Tier[1].AttrPtr = 0;
                                break;
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            throw new Exception("unexpected exception");
        }
        
        // 
        // 
        // 
        public void LoadURL(string XMLURL) {
            this.Load(XMLURL, 1);
        }
        
        // 
        // 
        // 
        public void LoadText(string XMLData) {
            this.Load(XMLData, 0);
        }
        
        // 
        // 
        // 
        public void LoadFile(string Filename) {
            this.Load(cpCore.appRootFiles.readFile(Filename), 0);
        }
        
        // 
        //  Get Name
        // 
        public string NodeName {
            get {
                NodeName = "";
                if ((TierPtr > 0)) {
                    if (!(Tier[TierPtr].Node == null)) {
                        NodeName = Tier[TierPtr].Node.Name;
                    }
                    
                }
                
            }
            set {
                if (Private_IsEmpty) {
                    TierPtr = 1;
                    MSxml = new XmlDocument();
                    object Tier;
                    Tier[TierPtr].Node = MSxml.CreateElement(value);
                    Private_IsEmpty = false;
                }
                else if (!(Tier[TierPtr].Node == null)) {
                    // Tier(TierPtr).Node.Name = vNewValue
                }
                
            }
        }
        
        public void GoNext() {
            try {
                int ParentPtr;
                // 
                if ((TierPtr > 0)) {
                    ParentPtr = (TierPtr - 1);
                    if (Tier[ParentPtr].ChildPtrOK) {
                        Tier[ParentPtr].ChildPtr = (Tier[ParentPtr].ChildPtr + 1);
                        Tier[ParentPtr].ChildPtrOK = (Tier[ParentPtr].Node.ChildNodes.Count > Tier[ParentPtr].ChildPtr);
                        Tier[TierPtr].Node = null;
                        Tier[TierPtr].AttrPtr = 0;
                        if (Tier[ParentPtr].ChildPtrOK) {
                            if ((Tier[ParentPtr].Node.ChildNodes(Tier[ParentPtr].ChildPtr).NodeType == System.Xml.XmlNodeType.Element)) {
                                Tier[TierPtr].Node = Tier[ParentPtr].Node.ChildNodes(Tier[ParentPtr].ChildPtr);
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        //  IsNodeOK
        // 
        public bool IsNodeOK() {
            IsNodeOK = false;
            if ((TierPtr > 0)) {
                IsNodeOK = Tier[(TierPtr - 1)].ChildPtrOK;
            }
            
        }
        
        // 
        //  Previous Node
        // 
        public void GoPrevious() {
            // 
            int ParentPtr;
            // 
            if ((TierPtr > 0)) {
                ParentPtr = (TierPtr - 1);
                if (Tier[ParentPtr].ChildPtrOK) {
                    Tier[ParentPtr].ChildPtr = (Tier[ParentPtr].ChildPtr + 1);
                    Tier[ParentPtr].ChildPtrOK = (Tier[ParentPtr].ChildPtr >= 0);
                    Tier[TierPtr].Node = null;
                    Tier[TierPtr].AttrPtr = 0;
                    if (Tier[ParentPtr].ChildPtrOK) {
                        if ((Tier[ParentPtr].Node.ChildNodes(Tier[TierPtr].ChildPtr).NodeType == System.Xml.XmlNodeType.Element)) {
                            Tier[TierPtr].Node = Tier[ParentPtr].Node.ChildNodes(Tier[TierPtr].ChildPtr);
                        }
                        
                    }
                    
                }
                
            }
            
        }
        
        // 
        // 
        // 
        public int ChildCount() {
            // 
            ChildCount = 0;
            if (!(Tier[TierPtr].Node == null)) {
                ChildCount = Tier[TierPtr].Node.ChildNodes.Count;
            }
            
        }
        
        // 
        // 
        // 
        public void GoFirstChild() {
            try {
                int Ptr;
                // 
                if (!(Tier[TierPtr].Node == null)) {
                    Tier[TierPtr].ChildPtr = 0;
                    Tier[TierPtr].ChildPtrOK = false;
                    if ((Tier[TierPtr].Node.ChildNodes.Count > 0)) {
                        // 
                        //  setup new tier
                        // 
                        TierPtr = (TierPtr + 1);
                        object Preserve;
                        Tier[TierPtr];
                        Tier[TierPtr].Node = Tier[(TierPtr - 1)].Node.ChildNodes(Ptr);
                        Tier[TierPtr].ChildPtr = 0;
                        Tier[TierPtr].ChildPtrOK = (Tier[(TierPtr - 1)].Node.ChildNodes.Count > 0);
                        Tier[TierPtr].AttrPtr = 0;
                        // 
                        //  set parent tier ptrs
                        // 
                        Tier[(TierPtr - 1)].ChildPtr = 0;
                        Tier[(TierPtr - 1)].ChildPtrOK = true;
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // 
        // 
        public void GoParent() {
            TierPtr = (TierPtr - 1);
        }
        
        // 
        // 
        // 
        public string GetAttrNames() {
            // 
            XmlAttribute NodeAttribute;
            // 
            GetAttrNames = "";
            if ((TierPtr > 0)) {
                foreach (NodeAttribute in Tier[TierPtr].Node.Attributes) {
                    GetAttrNames = (GetAttrNames + ("\r\n" + NodeAttribute.Name));
                    // End If
                }
                
            }
            
            if ((GetAttrNames != "")) {
                GetAttrNames = GetAttrNames.Substring(2);
            }
            
        }
        
        // 
        // 
        // 
        public void AddChild(string NodeName) {
            // 
            XmlNode ChildNode;
            // 
            if (Private_IsEmpty) {
                NodeName = "RootNode";
            }
            
            if ((TierPtr > 0)) {
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
            //     MSxml.documentElement
            // 
            //     Dim DocElement As xmlNode
            //     Dim ChildNode As xmlnode
            //     Dim Attr As xmlattribute
            //     '
            //     If TierPtr > 0 Then
            //         Attr = MSxml.createAttribute(AttrName)
            //         Tier(TierPtr).Node.seta
            //     End If
        }
        
        // 
        // 
        // 
        public int AttrCount() {
            // 
            AttrCount = 0;
            if (!(Tier[TierPtr].Node == null)) {
                if (!(Tier[TierPtr].Node.Attributes == null)) {
                    AttrCount = Tier[TierPtr].Node.Attributes.Count;
                }
                
            }
            
        }
        
        // 
        // 
        // 
        public void GoPreviousAttr() {
            // 
            if (this.IsAttrOK()) {
                Tier[TierPtr].AttrPtr = (Tier[TierPtr].AttrPtr - 1);
            }
            
        }
        
        // 
        // 
        // 
        public void GoNextAttr() {
            // 
            if (this.IsAttrOK()) {
                Tier[TierPtr].AttrPtr = (Tier[TierPtr].AttrPtr + 1);
            }
            
        }
        
        // 
        //  IsAttrOK
        // 
        public bool IsAttrOK() {
            IsAttrOK = false;
            if ((TierPtr > 0)) {
                if (!(Tier[TierPtr].Node == null)) {
                    IsAttrOK = ((Tier[TierPtr].AttrPtr >= 0) 
                                & (Tier[TierPtr].AttrPtr < Tier[TierPtr].Node.Attributes.Count));
                }
                
            }
            
        }
        
        // 
        // 
        // 
        public string GetAttr(string AttrName) {
            GetAttr = "";
            int AttrPtr;
            int AttrCnt;
            string UCaseAttrName;
            // 
            AttrCnt = Tier[TierPtr].Node.Attributes.Count;
            if (((AttrCnt > 0) 
                        && (AttrName != ""))) {
                UCaseAttrName = genericController.vbUCase(AttrName);
                for (AttrPtr = 0; (AttrPtr 
                            <= (AttrCnt - 1)); AttrPtr++) {
                    if ((UCaseAttrName == genericController.vbUCase(Tier[TierPtr].Node.Attributes(AttrPtr).Name))) {
                        GetAttr = Tier[TierPtr].Node.Attributes(AttrPtr).Value;
                        break;
                    }
                    
                }
                
            }
            
        }
        
        // 
        // 
        // 
        public string GetAttrName() {
            int AttrPtr;
            GetAttrName = "";
            if (this.IsAttrOK()) {
                AttrPtr = Tier[TierPtr].AttrPtr;
                GetAttrName = Tier[TierPtr].Node.Attributes(AttrPtr).Name;
            }
            
        }
        
        // 
        // 
        // 
        public string GetAttrValue() {
            int AttrPtr;
            GetAttrValue = "";
            if (this.IsAttrOK()) {
                AttrPtr = Tier[TierPtr].AttrPtr;
                GetAttrValue = Tier[TierPtr].Node.Attributes(AttrPtr).Value;
            }
            
        }
        
        // 
        public string XML {
            get {
                XML = "";
                if (!(Tier[TierPtr].Node == null)) {
                    XML = Tier[TierPtr].Node.InnerXml;
                }
                
            }
        }
        
        public string Text {
            get {
                Text = "";
                if (!(Tier[TierPtr].Node == null)) {
                    Text = Tier[TierPtr].Node.InnerText;
                }
                
            }
        }
        
        public void Clear() {
            Private_IsEmpty = true;
            if ((TierPtr <= 1)) {
                MSxml = null;
            }
            else if (!(Tier[TierPtr].Node == null)) {
                Tier[(TierPtr - 1)].Node.RemoveChild(Tier[TierPtr].Node);
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