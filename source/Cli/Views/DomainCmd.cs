
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;

namespace Contensive.CLI {
    //
    static class DomainCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--domain domain-to-be-used-for-application"
            + Environment.NewLine + "    The domain to be used as the primary domain for the application ( ex --domain mysite.sitefpo.com ). Requires you first set the application with -appname. Requires elevated permissions."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        /// <param name="cpServer"></param>
        /// <param name="appName"></param>
        /// <param name="domain">the argument following the command</param>
        public static void execute(Contensive.Processor.CPClass cpServer, string appName, string domain) {
            if (string.IsNullOrEmpty(appName)) {
                //
                // -- invalid argument
                Console.Write(Environment.NewLine + "Invalid argument. --domain requires you first set the application with -a appname.");
                Console.Write(helpText);
                Console.Write(Environment.NewLine + "Run cc --help for a full list of commands.");
                return;
            }
            if (string.IsNullOrEmpty(domain)) {
                //
                // -- invalid argument
                Console.Write(Environment.NewLine + "Invalid argument. --domain mustbe followed by the domain name you wish to use as the primary domain ( ex --domain mysite.sitefpo.com ).");
                Console.Write(helpText);
                Console.Write(Environment.NewLine + "Run cc --help for a full list of commands.");
                return;
            }
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(appName)) {
                //
                // -- set the app primary domain
                List<string> domainList = cp.GetAppConfig().domainList;
                if(domainList.Contains(domain)) {
                    domainList.Remove(domain);
                }
                domainList.Insert(0, domain);
                cp.core.serverConfig.save(cp.core);
                //
                // -- verify this binding is configured in the webserver
                cp.core.webServer.verifyWebsiteBinding(appName, domain);
                //
                // -- clear cache
                cp.Cache.InvalidateAll();
            }
        }
    }
}
