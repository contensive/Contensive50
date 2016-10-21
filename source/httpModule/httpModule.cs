using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.IO;
using System.Net;

namespace Contensive.Core
{
    public class httpModuleClass : IHttpModule 
    {
        public void  Dispose()
        {
        }
        /// <summary>
        ///BeginRequest
        ///AuthenticateRequest
        ///AuthorizeRequest
        ///ResolveRequestCache
        ///AcquireRequestState
        ///PreRequestHandlerExecute
        ///PostRequestHandlerExecute
        ///ReleaseRequestState
        ///UpdateRequestCache
        ///EndRequest
        ///
        ///PreSendRequestHeaders
        ///PreSendRequestContent
        ///Error
        /// </summary>
        /// <param name="app"></param>
        public bool IsReusable
        {
            get { return true; }
        }
        public void ProcessRequest(HttpContext context)
        {
            if (!File.Exists(context.Request.PhysicalPath))
            {
                throw new HttpException(404, String.Format("The file {0} does not exist", context.Request.PhysicalPath));
            }
            else
            {
                context.Response.TransmitFile(context.Request.PhysicalPath);
            }
        }
        public void Init(HttpApplication app ) 
        {
            // all work dobe in end
            //app.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            //app.AuthenticateRequest += (new EventHandler(this.Application_authenticateRequest));
            //app.PreRequestHandlerExecute += new EventHandler(OnPreRequestHandlerExecute);
            app.EndRequest += (new EventHandler(this.Application_EndRequest));
            //app.Error += (new EventHandler(this.Application_Error));
        }
        //
        // BeginRequest: Request has been started. If you need to do something at the beginning of a request (for example, display advertisement banners at the top of each page), synchronize this event.
        //
        private void Application_BeginRequest(object appSource, EventArgs e)
        {
            try
            {
                HttpApplication app = (HttpApplication)appSource;
                HttpContext context = app.Context;
                string filePath = context.Request.FilePath;
                context.Response.Write("<div>beginRequest, filePath=" + filePath + "</div>");
                string fileExtension = VirtualPathUtility.GetExtension(filePath);
                context.Response.Write("<div>" + version() + ", beginRequest, fileExtension=" + fileExtension.ToString() + "</div>");
                //if (fileExtension.Equals(".html"))
                //{
                //    context.Response.Write("<div>beginRequest, fileExtension=" + fileExtension.ToString() + "</div>");
                //}
            }
            catch (Exception ex)
            {
            }
        }
        //
        // BeginRequest: Request has been started. If you need to do something at the beginning of a request (for example, display advertisement banners at the top of each page), synchronize this event.
        //
        private void Application_authenticateRequest(Object appSource, EventArgs e)
        {
            HttpApplication app = (HttpApplication)appSource;
            HttpContext context = app.Context;
            context.Response.Write("<div>" + version() + ", AuthenticateRequest</div>");
        }
        //
        // OnPreRequestHandlerExecute
        //
        public void OnPreRequestHandlerExecute(    Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            HttpRequest request = app.Context.Request;
            HttpContext context = app.Context;
            context.Response.Write("<div>" + version() + ", AuthenticateRequest</div>");

            if (!String.IsNullOrEmpty(request.Headers["Referer"]))
            {
                throw new HttpException(403,"Uh-uh!");
            }
        }
        //
        // EndRequest: Request has been completed. You may want to build a debugging module that gathers information throughout the request and then writes the information to the page.
        //
        // 404 errors bypass all the other methods, so this is where we will handle the interception
        //
        private void Application_EndRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);
            
            //var context = (HttpApplication)sender;
            if(context.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                //context.Response.Redirect("http://michiel.vanotegem.nl/");
                context.Response.Write("<div>" + version() + ", EndRequest-return cfw5 html render here</div>");
            }
            
        }
        //
        // Error:
        //
        private void Application_Error(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            context.Response.Write("<div>" + version() + ", Error</div>");
            
        }
        //
        //
        //
        private string version()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
           
        }
    }
}
