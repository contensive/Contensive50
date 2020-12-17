
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    /// <summary>
    /// Image methods
    /// </summary>
    public abstract class CPImageBaseClass {
        //
        //==========================================================================================
        /// <summary>
        /// Return an image resized and cropped to best fit the hole. 
        /// The source imagePathFilename is expected to be in the CdnFiles filesystem. 
        /// New image is saved back to the same path.  
        /// imageAltSizeList is a list of strings, each representing a previous resize of this image in the format "filename-000x000". The method checks this list for the size requested and if found returns success. 
        /// If the size requested is not in the list, it is created, saved to the path, and this new size is added to the list. On return, if the imageAltSizeList.count changes, save this new list with the image for future calls. 
        /// Test if in cache first, else load record and test for resized in alt list, else Resize the image and save back to the image's record
        /// </summary>
        /// <param name="imagePathFilename"></param>
        /// <param name="holeWidth"></param>
        /// <param name="holeHeight"></param>
        /// <param name="imageAltSizeList"></param>
        public abstract string GetBestFit(string imagePathFilename, int holeWidth, int holeHeight, List<string> imageAltSizeList);
        //
        //====================================================================================================
        // deprecated
        //
    }
}

