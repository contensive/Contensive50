
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Runtime.InteropServices;
//====================================================================================================
// Convensions:
//
// -- for these abstract base classes
//      PascalCase all properties and methods 
//      legacy case, case changes will break C# code written against these classes
//      be consistent with new methods
//
// -- for code in general
//https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions
//   - camelCase arguemnts and properties
//   - PascalCase everything else
//
namespace Contensive.BaseClasses {
    //
    //====================================================================================================
    /// <summary>
    /// CP - The object passed to an addon in the add-ons execute method. See the AddonBaseClass for details of the addon execute method.
    /// </summary>
    /// <remarks></remarks>
    public abstract class CPBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Construct new Block object. See CPBlockBaseClass for Block Details
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPBlockBaseClass BlockNew();
        //
        //====================================================================================================
        /// <summary>
        /// Construct new CS object. See CPCSBaseClass for CS object details 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPCSBaseClass CSNew();
        //
        //====================================================================================================
        /// <summary>
        /// Contensive version
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string Version { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Group Object accesses group features. Group Features generally associate people and roles. See CPGroupBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPGroupBaseClass Group { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Request object handles data associated with the request from the visitor. See CPRequestBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPRequestBaseClass Request { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Response object handles the stream of data back to the visitor. See CPResponseBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPResponseBaseClass Response { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The UserError Class handles error handling for those conditions you want the user to know about or correct. For example an login error. See the CPUserErrorBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPUserErrorBaseClass UserError { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Visit Class handles details related to the visit. For instance it holds the number of pages hit so far and has methods for adding and modifying user defined visit properties. See CPVisitBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPVisitBaseClass Visit { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Visitor Class handles details related to the visitor. For instance it holds the browser type used by the visitor. See CPVisitorBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPVisitorBaseClass Visitor { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The User Class handles details related to the user and its related people record. See CPUserBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPUserBaseClass User { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The HTML class handles functions used to read and produce HTML elements. See CPHtmlBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPHtmlBaseClass Html { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Cache objects handles caching. Use this class to save blocks of data you will use again. See CPCacheBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPCacheBaseClass Cache { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Db object handles direct access to the Database. The ContentSet functions in the CPCSBaseClass are prefered for general use. See the CPDBBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPDbBaseClass Db { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Email object handles email functions. See CPEmailBaseClass for more information.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPEmailBaseClass Email { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Content class handles functions related to content meta such as determining the table used for a content definition, getting a recordid based on the name, or accessing the methods that control workflow publishing. See CPContentBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPContentBaseClass Content { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The addon class handles access to an add-on's features. Use the Utils object to run an cpcore.addon. An instance of the Addon class is passed to the executing addon in the MyAddon object so it can access any features needed. See the CPAddonBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPAddonBaseClass Addon { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Utils class handles basic utilities and other features not classified. See CPUtilsBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPUtilsBaseClass Utils { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Doc object handles features related to the document (page) being contructed in the current call. See CPDocBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPDocBaseClass Doc { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The Site Class handles features related to the current site. See CPSiteBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPSiteBaseClass Site { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The MyAddon object is an instance of the Addon class created before an add-ons execute method is called. See CPAddonBaseClass for more details.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract CPAddonBaseClass MyAddon { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Legacy file object. Use cdnFiles, wwwFiles, privateFiles and tempFiles.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Legacy file object. Use cdnFiles, wwwFiles, privateFiles and tempFiles.")]
        public abstract CPFileBaseClass File { get; }
        //
        //====================================================================================================
        /// <summary>
        /// read and write cdn files, like content uploads. Sites with static front-ends may put static files here.
        /// </summary>
        /// <returns></returns>
        public abstract CPFileSystemBaseClass CdnFiles { get; }
        //
        //====================================================================================================
        /// <summary>
        /// read and write files in the root folder of the application (appRoot, wwwRoot,htdocs,etc)
        /// </summary>
        /// <returns></returns>
        public abstract CPFileSystemBaseClass WwwFiles { get; }
        //
        //====================================================================================================
        /// <summary>
        /// read and write files not available to the Internet
        /// </summary>
        /// <returns></returns>
        public abstract CPFileSystemBaseClass PrivateFiles { get; }
        //
    }

}
