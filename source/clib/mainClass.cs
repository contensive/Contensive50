using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Core
{
    class mainClass
    {
        static void Main(string[] args)
        {
            CPClass cp;
            //
            // create cp for cluster work, with no application
            //
            if (args.Length == 0)
            {
                Console.WriteLine(helpText); // Check for null array
            }
            else
            {
                //Console.Write("args length is ");
                //Console.WriteLine(args.Length); // Write array length

                bool exitCmd = false;
                for (int i = 0; i < args.Length; i++) // Loop through array
                {
                    string argument = args[i];
                    switch (argument.ToLower())
                    {
                        case "-version":
                        case "-v":
                            cp = new CPClass("");
                            Console.WriteLine("version " + cp.core.version() );
                            exitCmd = true;
                            break;
                        case "-newapp":
                        case "-n":
                            createAppClass createApp = new createAppClass();
                            createApp.createApp();
                            exitCmd = true;
                            break;
                        case "-upgrade": case "-u":
                            string appName;
                            if ( i == (args.Length+1))
                            {
                                Console.WriteLine("Application name?");
                                appName = Console.ReadLine();
                            } else {
                                i++;
                                appName = args[i];
                            }
                            if ( string.IsNullOrEmpty(appName ))
                            {
                                Console.WriteLine("ERROR: upgrade requires a valid app name.");
                                i = args.Length;
                            } else {
                                cp = new CPClass(appName);
                                builderClass builder = new builderClass(cp.core);
                                builder.upgrade(false);
                            }
                            exitCmd = true;
                            break;
                        default:
                            Console.Write(helpText);
                            exitCmd = true;
                            break;
                    }
                    if (exitCmd) break;
                }
            }
        }
        const string helpText = ""
            + "\r\nclib command line"
            + "\r\n"
            + "\r\n-n"
            + "\r\n\tnew application wizard"
            + "\r\n"
            + "\r\n-u appName"
            + "\r\n\trun application upgrade"
            + "";
    }
}
