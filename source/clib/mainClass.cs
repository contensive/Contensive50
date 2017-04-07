using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Core
{
    class mainClass
    {
        static  void Main(string[] args)
        {
            try
            { 
                coreFileSystemClass installFiles;
                string appName;
                //
                // create cp for cluster work, with no application
                //
                //System.IO.File.WriteAllText("c:\\tmp\\clib.log", String.Join(",",args) );
                if (args.Length == 0)
                {
                    Console.WriteLine(helpText); // Check for null array
                }
                else
                {
                    //
                    // -- create an instance of cp to execute commands
                    using (CPClass cp = new CPClass())
                    {
                        //
                        // start by creating an application event log entry - because you must be admin to make this entry so making it here will create the "source"
                        string eventLogSource = "Contensive";
                        string eventLogLog = "Application";
                        string eventLogEvent = "command line:";
                        for (int argPtr = 0; argPtr < args.Length; argPtr++)
                        {
                            eventLogEvent += " " + args[argPtr];
                        }
                        if (!EventLog.SourceExists(eventLogSource)) EventLog.CreateEventSource(eventLogSource, eventLogLog);
                        EventLog.WriteEntry(eventLogSource, eventLogEvent, EventLogEntryType.Information);
                        //
                        // -- set programfiles path if emptry
                        if (String.IsNullOrEmpty(cp.core.serverConfig.programFilesPath))
                        {
                            string executePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                            if (executePath.ToLower().IndexOf("\\git\\") == 0)
                            {
                                //  -- save if not in developer execution path
                                cp.core.serverConfig.programFilesPath = executePath;
                            }
                            else
                            {
                                //  -- developer, fake a path
                                cp.core.serverConfig.programFilesPath = "c:\\Program Files (x86)\\kma\\Contensive5\\";
                            }
                            cp.core.serverConfig.saveObject(cp.core);
                        }
                        //
                        // -- loop through arguments and execute each command
                        for (int argPtr = 0; argPtr < args.Length; argPtr++) // Loop through array
                        {
                            createAppClass createApp = new createAppClass();
                            string argument = args[argPtr];
                            bool exitArgumentProcessing = false;
                            switch (argument.ToLower())
                            {
                                case "--version":
                                case "-v":
                                    //
                                    // display core version
                                    Console.WriteLine("version " + cp.core.common_version());
                                    exitArgumentProcessing = true;
                                    break;
                                case "--newapp":
                                case "-n":
                                    //
                                    // -- start the new app wizard
                                    createApp.createApp();
                                    exitArgumentProcessing = true;
                                    break;
                                case "--configure":
                                    //
                                    // -- eventually write a configure. For now, just use the new app
                                    configureClass.configure();
                                    exitArgumentProcessing = true;
                                    break;
                                case "--status":
                                case "-s":
                                    //
                                    // -- display ServerGroup and application status
                                    if (!cp.serverOk)
                                    {
                                        //
                                        // -- something went wrong with server initialization
                                        Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] not found or not valid. Run clib --configure");
                                    }
                                    else
                                    {
                                        Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] found.");
                                        Console.WriteLine("ServerGroup name: " + cp.core.serverConfig.name);
                                        Console.WriteLine("ElastiCacheConfigurationEndpoint: " + cp.core.serverConfig.awsElastiCacheConfigurationEndpoint);
                                        Console.WriteLine("cdnFilesRemoteEndpoint: " + cp.core.serverConfig.cdnFilesRemoteEndpoint);
                                        Console.WriteLine("isLocal: " + cp.core.serverConfig.isLocalFileSystem.ToString());
                                        Console.WriteLine("defaultDataSourceAddress: " + cp.core.serverConfig.defaultDataSourceAddress.ToString());
                                        Console.WriteLine("defaultDataSourceType: " + cp.core.serverConfig.defaultDataSourceType.ToString());
                                        Console.WriteLine("defaultDataSourceUsername: " + cp.core.serverConfig.defaultDataSourceUsername.ToString());
                                        Console.WriteLine("isLocalCache: " + cp.core.serverConfig.isLocalCache.ToString());
                                        Console.WriteLine("maxConcurrentTasksPerServer: " + cp.core.serverConfig.maxConcurrentTasksPerServer.ToString());
                                        Console.WriteLine("apps.Count: " + cp.core.serverConfig.apps.Count);
                                        foreach (KeyValuePair<string, Models.Entity.serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps)
                                        {
                                            Models.Entity.serverConfigModel.appConfigModel app = kvp.Value;
                                            Console.WriteLine("----------app name: " + app.name);
                                            Console.WriteLine("\tenabled: " + app.enabled);
                                            Console.WriteLine("\tadminRoute: " + app.adminRoute);
                                            Console.WriteLine("\tappRootPath: " + app.appRootFilesPath);
                                            Console.WriteLine("\tprivateFilesPath: " + app.privateFilesPath);
                                            Console.WriteLine("\tcdnFilesPath: " + app.cdnFilesPath);
                                            Console.WriteLine("\tcdnFilesNetprefix: " + app.cdnFilesNetprefix);
                                            foreach (string domain in app.domainList)
                                            {
                                                Console.WriteLine("\tdomain: " + domain);
                                            }
                                            Console.WriteLine("\tenableCache: " + app.enableCache);
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--upgrade":
                                case "-u":
                                    //
                                    // -- upgrade the app in the argument list, or prompt for it
                                    if (argPtr == (args.Length + 1))
                                    {
                                        Console.WriteLine("Application name?");
                                        appName = Console.ReadLine();
                                    }
                                    else
                                    {
                                        argPtr++;
                                        appName = args[argPtr];
                                    }
                                    if (string.IsNullOrEmpty(appName))
                                    {
                                        Console.WriteLine("ERROR: upgrade requires a valid app name.");
                                        argPtr = args.Length;
                                    }
                                    else
                                    {
                                        installFiles = new coreFileSystemClass(cp.core, cp.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                                        Controllers.coreBuilderClass.upgrade(cp.core,false);
                                        installFiles.Dispose();
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--upgradeall":
                                    //
                                    // upgrade all apps in the server group
                                    foreach (KeyValuePair<String, Models.Entity.serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps)
                                    {
                                        String upgradeAppName = kvp.Key;
                                        using (CPClass upgradeApp = new CPClass(upgradeAppName))
                                        {
                                            Controllers.coreBuilderClass.upgrade(cp.core,false);
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--taskscheduler":
                                    //
                                    // -- manage the task scheduler
                                    if (argPtr != (args.Length + 1))
                                    {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run")
                                        {
                                            //
                                            // run the taskscheduler in the console
                                            Console.WriteLine("Beginning command line taskScheduler. Hit any key to exit");
                                            coreTaskSchedulerServiceClass taskScheduler = new coreTaskSchedulerServiceClass();
                                            taskScheduler.allowVerboseLogging = true;
                                            taskScheduler.allowConsoleWrite = true;
                                            taskScheduler.StartService(true, false);
                                            object keyStroke = Console.ReadKey();
                                            taskScheduler.stopService();
                                            exitArgumentProcessing = true;
                                        }
                                        else
                                        {
                                            //
                                            // turn the windows service scheduler on/off
                                            cp.core.serverConfig.allowTaskSchedulerService = cp.Utils.EncodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowtaskscheduler set " + cp.core.serverConfig.allowTaskSchedulerService.ToString());
                                        }
                                    }
                                    break;
                                case "--taskrunner":
                                    //
                                    // -- manager the task runner
                                    if (argPtr != (args.Length + 1))
                                    {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run")
                                        {
                                            //
                                            // -- run the taskrunner in the console
                                            Console.WriteLine("Beginning command line taskRunner. Hit any key to exit");
                                            coreTaskRunnerServiceClass taskRunner = new coreTaskRunnerServiceClass();
                                            taskRunner.allowVerboseLogging = true;
                                            taskRunner.allowConsoleWrite = true;
                                            taskRunner.StartService();
                                            object keyStroke = Console.ReadKey();
                                            taskRunner.stopService();
                                            exitArgumentProcessing = true;
                                        }
                                        else
                                        {
                                            //
                                            // -- turn the windows service scheduler on/off
                                            cp.core.serverConfig.allowTaskRunnerService = cp.Utils.EncodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowtaskrunner set " + cp.core.serverConfig.allowTaskRunnerService.ToString());
                                        }
                                    }
                                    break;
                                case "--tasks":
                                    //
                                    // -- turn on, off or run both services together
                                    if (argPtr != (args.Length + 1))
                                    {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run")
                                        {
                                            //
                                            // run the tasks in the console
                                            //
                                            Console.WriteLine("Beginning command line taskScheduler and taskRunner. Hit any key to exit");
                                            //
                                            coreTaskSchedulerServiceClass taskScheduler = new coreTaskSchedulerServiceClass();
                                            taskScheduler.allowVerboseLogging = true;
                                            taskScheduler.allowConsoleWrite = true;
                                            taskScheduler.StartService(true, false);
                                            //
                                            coreTaskRunnerServiceClass taskRunner = new coreTaskRunnerServiceClass();
                                            taskRunner.allowVerboseLogging = true;
                                            taskRunner.allowConsoleWrite = true;
                                            taskRunner.StartService();
                                            //
                                            object keyStroke = Console.ReadKey();
                                            taskRunner.stopService();
                                            exitArgumentProcessing = true;
                                        }
                                        else
                                        {
                                            //
                                            // turn the windows service scheduler on/off
                                            //
                                            cp.core.serverConfig.allowTaskSchedulerService = cp.Utils.EncodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.allowTaskRunnerService = cp.Utils.EncodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowTaskScheduler set " + cp.core.serverConfig.allowTaskSchedulerService.ToString());
                                            Console.WriteLine("allowTaskRunner set " + cp.core.serverConfig.allowTaskRunnerService.ToString());
                                        }
                                    }
                                    break;
                                default:
                                    Console.Write(helpText);
                                    exitArgumentProcessing = true;
                                    break;
                            }
                            if (exitArgumentProcessing) break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error that forced the program to close. Details follow.\n\n" + ex.ToString());
            }
        }
        const string helpText = ""
            + "\r\nclib command line"
            + "\r\n"
            + "\r\n-newapp (-n)"
            + "\r\n\tnew application wizard"
            + "\r\n"
            + "\r\n-upgrade appName (-u appname)"
            + "\r\n\trun application upgrade"
            + "\r\n"
            + "\r\n-upgradeall"
            + "\r\n\trun application upgrade on all applications"
            + "\r\n"
            + "\r\n-version (-v)"
            + "\r\n\tdisplay code version"
            + "\r\n"
            + "\r\n-status (-s)"
            + "\r\n\tdisplay configuration status"
            + "\r\n"
            + "\r\n-taskscheduler run"
            + "\r\n\tRun the taskscheduler in the console (temporary)"
            + "\r\n"
            + "\r\n-taskscheduler on|off"
            + "\r\n\tStart or stop the taskscheduler service"
            + "\r\n"
            + "\r\n-taskrunner run"
            + "\r\n\tRun the taskrunner in the console (temporary)"
            + "\r\n"
            + "\r\n-taskrunner on|off"
            + "\r\n\tStart or stop the taskrunner service"
            + "";
    }

}
