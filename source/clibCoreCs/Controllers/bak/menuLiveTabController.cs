


namespace Controllers {
    
    public class menuLiveTabController {
        
        // 
        private struct TabType {
            
            private string Caption;
            
            private string StylePrefix;
            
            private string LiveBody;
        }
        
        private TabType[] Tabs;
        
        private int TabsCnt;
        
        private int TabsSize;
        
        // 
        private menuComboTabController comboTab = new menuComboTabController();
        
        // 
        public void AddEntry(string Caption, string LiveBody, string StylePrefix, void =, void ) {
            comboTab.AddEntry(Caption, "", "", LiveBody, false, "ccAdminTab");
            // Warning!!! Optional parameters not supported
        }
        
        // 
        public string GetTabs() {
            return comboTab.GetTabs;
        }
        
        // 
        public string GetTabBlank() {
            return comboTab.GetTabBlank;
        }
    }
}