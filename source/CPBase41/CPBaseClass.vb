Imports System.Runtime.InteropServices
'====================================================================================================
' Convensions:
'https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions
'   - camelCase arguemnts and properties
'   - PascalCase everything else
Namespace Contensive.BaseClasses
    '
    '====================================================================================================
    ''' <summary>
    ''' CP - The object passed to an addon in the add-ons execute method. See the AddonBaseClass for details of the addon execute method.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CPBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Factory for new Block object. See CPBlockBaseClass for Block Details
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function BlockNew() As CPBlockBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Factory for new CS object. See CPCSBaseClass for CS object details 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function CSNew() As CPCSBaseClass 'Implements BaseClasses.CPBaseClass.CSNew
        '
        '====================================================================================================
        ''' <summary>
        ''' Contensive version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Version() As String 'Implements BaseClasses.CPBaseClass.Version
        '
        '====================================================================================================
        ''' <summary>
        ''' The Group Object accesses group features. Group Features generally associate people and roles. See CPGroupBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Group() As CPGroupBaseClass 'Implements BaseClasses.CPBaseClass.Group
        '
        '====================================================================================================
        ''' <summary>
        ''' The Request object handles data associated with the request from the visitor. See CPRequestBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Request() As CPRequestBaseClass 'Implements BaseClasses.CPBaseClass.Request
        '
        '====================================================================================================
        ''' <summary>
        ''' The Response object handles the stream of data back to the visitor. See CPResponseBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Response() As CPResponseBaseClass 'Implements BaseClasses.CPBaseClass.Response
        '
        '====================================================================================================
        ''' <summary>
        ''' The UserError Class handles error handling for those conditions you want the user to know about or correct. For example an login error. See the CPUserErrorBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property UserError() As CPUserErrorBaseClass 'Implements BaseClasses.CPBaseClass.UserError
        '
        '====================================================================================================
        ''' <summary>
        ''' The Visit Class handles details related to the visit. For instance it holds the number of pages hit so far and has methods for adding and modifying user defined visit properties. See CPVisitBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Visit() As CPVisitBaseClass 'Implements BaseClasses.CPBaseClass.Visit
        '
        '====================================================================================================
        ''' <summary>
        ''' The Visitor Class handles details related to the visitor. For instance it holds the browser type used by the visitor. See CPVisitorBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Visitor() As CPVisitorBaseClass 'Implements BaseClasses.CPBaseClass.Visitor
        '
        '====================================================================================================
        ''' <summary>
        ''' The User Class handles details related to the user and its related people record. See CPUserBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property User() As CPUserBaseClass 'Implements BaseClasses.CPBaseClass.User
        '
        '====================================================================================================
        ''' <summary>
        ''' The HTML class handles functions used to read and produce HTML elements. See CPHtmlBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Html() As CPHtmlBaseClass 'Implements BaseClasses.CPBaseClass.Html
        '
        '====================================================================================================
        ''' <summary>
        ''' The Cache objects handles caching. Use this class to save blocks of data you will use again. See CPCacheBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Cache() As CPCacheBaseClass 'Implements BaseClasses.CPBaseClass.Cache
        '
        '====================================================================================================
        ''' <summary>
        ''' The Db object handles direct access to the Database. The ContentSet functions in the CPCSBaseClass are prefered for general use. See the CPDBBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Db() As CPDbBaseClass 'Implements BaseClasses.CPBaseClass.Db
        '
        '====================================================================================================
        ''' <summary>
        ''' The Email object handles email functions. See CPEmailBaseClass for more information.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Email() As CPEmailBaseClass 'Implements BaseClasses.CPBaseClass.Email
        '
        '====================================================================================================
        ''' <summary>
        ''' The Content class handles functions related to content meta such as determining the table used for a content definition, getting a recordid based on the name, or accessing the methods that control workflow publishing. See CPContentBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Content() As CPContentBaseClass 'Implements BaseClasses.CPBaseClass.Content
        '
        '====================================================================================================
        ''' <summary>
        ''' The addon class handles access to an add-on's features. Use the Utils object to run an cpcore.addon. An instance of the Addon class is passed to the executing addon in the MyAddon object so it can access any features needed. See the CPAddonBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Addon() As CPAddonBaseClass 'Implements BaseClasses.CPBaseClass.Addon
        '
        '====================================================================================================
        ''' <summary>
        ''' The Utils class handles basic utilities and other features not classified. See CPUtilsBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Utils() As CPUtilsBaseClass 'Implements BaseClasses.CPBaseClass.Utils
        '
        '====================================================================================================
        ''' <summary>
        ''' The Doc object handles features related to the document (page) being contructed in the current call. See CPDocBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Doc() As CPDocBaseClass 'Implements BaseClasses.CPBaseClass.Doc
        '
        '====================================================================================================
        ''' <summary>
        ''' The Site Class handles features related to the current site. See CPSiteBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property Site() As CPSiteBaseClass 'Implements BaseClasses.CPBaseClass.Site
        '
        '====================================================================================================
        ''' <summary>
        ''' The MyAddon object is an instance of the Addon class created before an add-ons execute method is called. See CPAddonBaseClass for more details.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property MyAddon() As CPAddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Legacy file object. Use cdnFiles, wwwFiles, privateFiles and tempFiles.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Legacy file object. Use cdnFiles, wwwFiles, privateFiles and tempFiles.")> Public MustOverride ReadOnly Property File() As CPFileBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' read and write cdn files, like content uploads
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property cdnFiles() As CPFileSystemBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' read and write files in the root folder of the application (wwwRoot,htdocs,etc)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property wwwFiles() As CPFileSystemBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' read and write files not available to the Internet
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property privateFiles() As CPFileSystemBaseClass
        '
    End Class

End Namespace
