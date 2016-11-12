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
            CPClass cpCluster;
            fileSystemClass installFiles;
            fileSystemClass programDataFiles;
            string appName;
            string JSONTemp;
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
                    createAppClass createApp = new createAppClass();
                    string argument = args[i];
                    switch (argument.ToLower())
                    {
                        case "-version":
                        case "-v":
                            //
                            // display core version
                            //
                            cp = new CPClass("");
                            Console.WriteLine("version " + cp.core.version() );
                            exitCmd = true;
                            break;
                        case "-newapp":
                        case "-n":
                            //
                            // start the new app wizard
                            //
                            createApp.createApp();
                            exitCmd = true;
                            break;
                        case "-upgrade": case "-u":
                            //
                            // upgrade the app in the argument list, or prompt for it
                            //
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
                                installFiles = new fileSystemClass(cp.core, cp.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                                Console.WriteLine("Upgrading cluster folder clibResources from installation");
                                createApp.upgradeResources(cp, installFiles);
                                builderClass builder = new builderClass(cp.core);
                                builder.upgrade(false);
                            }
                            exitCmd = true;
                            break;
                        case "-upgradeall":
                            //
                            // upgrade all apps in the cluster
                            //
                            cpCluster = new CPClass("");
                            installFiles = new fileSystemClass(cpCluster.core, cpCluster.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                            Console.WriteLine("Upgrading cluster folder clibResources from installation");
                            createApp.upgradeResources(cpCluster, installFiles);
                            //
                            foreach (var item in cpCluster.core.cluster.config.apps)
                            {
                                cp = new CPClass(item.Key);
                                builderClass builder = new builderClass(cp.core);
                                builder.upgrade(false);
                            }
                            exitCmd = true;
                            break;
                        case "-taskscheduler":
                            cpCluster = new CPClass("");
                            programDataFiles = new fileSystemClass(cpCluster.core, cpCluster.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib");
                            JSONTemp = programDataFiles.ReadFile("serverConfig.json");
                            if (string.IsNullOrEmpty(JSONTemp))
                            {
                                Console.WriteLine("The serverConfig.json file was no found in c:\\programData\\clib. Please run -n to initialize the server.");
                            }
                            else
                            {
                                System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                serverConfigClass serverConfig = json_serializer.Deserialize<serverConfigClass>(JSONTemp);
                                if (i != (args.Length + 1))
                                {
                                    i++;
                                    if (args[i].ToLower() == "run")
                                    {
                                        //
                                        // run the taskscheduler in the console
                                        //
                                        Console.WriteLine("Beginning command line taskScheduler. Hit any key to exit");
                                        taskSchedulerServiceClass taskScheduler = new taskSchedulerServiceClass(cpCluster.core);
                                        taskScheduler.StartService(true, false);
                                        object keyStroke = Console.ReadKey();
                                        taskScheduler.stopService();
                                        exitCmd = true;
                                    }
                                    else
                                    {
                                        //
                                        // turn the windows service scheduler on/off
                                        //
                                        serverConfig.allowTaskSchedulerService = cpCluster.Utils.EncodeBoolean(args[i]);
                                        Console.WriteLine("allowtaskscheduler set " + serverConfig.allowTaskSchedulerService.ToString());
                                        programDataFiles.SaveFile("serverConfig.json", json_serializer.Serialize(serverConfig));
                                    }
                                }
                            }
                            break;
                        case "-taskrunner":
                            cpCluster = new CPClass("");
                            programDataFiles = new fileSystemClass(cpCluster.core, cpCluster.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib");
                            JSONTemp = programDataFiles.ReadFile("serverConfig.json");
                            if (string.IsNullOrEmpty(JSONTemp))
                            {
                                Console.WriteLine("The serverConfig.json file was no found in c:\\programData\\clib. Please run -n to initialize the server.");
                            }
                            else
                            {
                                System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                serverConfigClass serverConfig = json_serializer.Deserialize<serverConfigClass>(JSONTemp);
                                if (i != (args.Length + 1))
                                {
                                    i++;
                                    if (args[i].ToLower() == "run")
                                    {
                                        //
                                        // run the taskrunner in the console
                                        //
                                        Console.WriteLine("Beginning command line taskRunner. Hit any key to exit");
                                        taskRunnerServiceClass taskRunner = new taskRunnerServiceClass(cpCluster.core);
                                        taskRunner.StartService();
                                        object keyStroke = Console.ReadKey();
                                        taskRunner.stopService();
                                        exitCmd = true;
                                    }
                                    else
                                    {
                                        //
                                        // turn the windows service scheduler on/off
                                        //
                                        serverConfig.allowTaskRunnerService = cpCluster.Utils.EncodeBoolean(args[i]);
                                        Console.WriteLine("allowtaskrunner set " + serverConfig.allowTaskRunnerService.ToString());
                                        programDataFiles.SaveFile("serverConfig.json", json_serializer.Serialize(serverConfig));
                                    }
                                }
                            }
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
