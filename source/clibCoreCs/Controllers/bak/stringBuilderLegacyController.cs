


namespace Controllers {
    
    public class stringBuilderLegacyController {
        
        // 
        private int iSize;
        
        private int iCount;
        
        private string[] Holder;
        
        // 
        // ==========================================================================================
        // '' <summary>
        // '' add a string to the stringbuilder
        // '' </summary>
        // '' <param name="NewString"></param>
        public void Add(string NewString) {
            try {
                if ((iCount >= iSize)) {
                    iSize = (iSize + iChunk);
                    object Preserve;
                    Holder[iSize];
                }
                
                Holder[iCount] = NewString;
                iCount = (iCount + 1);
            }
            catch (Exception ex) {
                throw new ApplicationException("Exception in coreFastString.Add()", ex);
            }
            
        }
        
        // 
        // ==========================================================================================
        // '' <summary>
        // '' read the string out of the string builder
        // '' </summary>
        // '' <returns></returns>
        public string Text {
            get {
                return (Join(Holder, "") + "");
            }
        }
    }
}