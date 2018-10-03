
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Models.Db;
using Contensive.Processor;

namespace Contensive.CLI {
    class mainClass {
        static void Main(string[] args) {
            try {
                string appName = "";
                string collectionName = "";
                //
                // create cp for cluster work, with no application
                //
                if (args.Length == 0) {
                    Console.WriteLine(helpText); // Check for null array
                } else {
                    //
                    // -- create an instance of cp to execute commands
                    using (Processor.CPClass cpServer = new Processor.CPClass()) {
                        //
                        // -- if logging enabled, tell user the output includes log append
                        if (cpServer.core.serverConfig.enableLogging) {
                            Console.WriteLine("Logging enabled, all internal logging will be included.");
                        }
                        //
                        // start by creating an application event log entry - because you must be admin to make this entry so making it here will create the "source"
                        string eventLogSource = "Contensive";
                        string eventLogLog = "Application";
                        string eventLogEvent = "command line:";
                        for (int argPtr = 0; argPtr < args.Length; argPtr++) {
                            eventLogEvent += " " + args[argPtr];
                        }
                        if (!EventLog.SourceExists(eventLogSource)) EventLog.CreateEventSource(eventLogSource, eventLogLog);
                        EventLog.WriteEntry(eventLogSource, eventLogEvent, EventLogEntryType.Information);
                        //
                        // -- set programfiles path if empty
                        if (String.IsNullOrEmpty(cpServer.core.serverConfig.programFilesPath)) {
                            string executePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                            if (executePath.ToLower().IndexOf("\\git\\") == 0) {
                                //  -- save if not in developer execution path
                                cpServer.core.serverConfig.programFilesPath = executePath;
                            } else {
                                //  -- developer, fake a path
                                cpServer.core.serverConfig.programFilesPath = "c:\\Program Files (x86)\\kma\\Contensive5\\";
                            }
                            cpServer.core.serverConfig.saveObject(cpServer.core);
                        }
                        //
                        // -- loop through arguments and execute each command
                        for (int argPtr = 0; argPtr < args.Length; argPtr++) {
                            string argument = args[argPtr];
                            bool exitArgumentProcessing = false;
                            bool repair = false;
                            switch (argument.ToLower()) {
                                case "--flushcache":
                                    if (!string.IsNullOrEmpty(appName)) {
                                        //
                                        // -- flush this app
                                        using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                                            cpApp.Cache.InvalidateAll();
                                        }
                                    } else {
                                        //
                                        // -- flush all apps
                                        foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                                            String housekeepAppName = kvp.Key;
                                            using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(housekeepAppName)) {
                                                cpApp.Cache.InvalidateAll();
                                            }
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "-a":
                                    //
                                    // set application name
                                    if (argPtr < (args.Length + 1)) {
                                        argPtr++;
                                        appName = args[argPtr];
                                    }
                                    break;
                                case "-i":
                                case "--install":
                                    //
                                    // -- install collection to one or all applications
                                    if (argPtr < (args.Length + 1)) {
                                        argPtr++;
                                        collectionName = args[argPtr];
                                    }
                                    if (string.IsNullOrEmpty(collectionName)) {
                                        Console.WriteLine("Collection name is required after the --install command");
                                    } else {
                                        //
                                        // -- determine guid of collection
                                        var collectionList = new List<CollectionController.CollectionStoreClass>();
                                        CollectionController.getRemoteCollectionStoreList(cpServer.core, ref collectionList);
                                        string collectionGuid = "";
                                        foreach (var collection in collectionList) {
                                            if (collection.name.ToLower() == collectionName.ToLower()) {
                                                collectionGuid = collection.guid;
                                                break;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(collectionGuid)) {
                                            Console.WriteLine("Collection was not found on the distribution server");
                                        } else {
                                            if (string.IsNullOrEmpty(appName)) {
                                                foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                                                    using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(kvp.Key)) {
                                                        string returnErrorMessage = "";
                                                        CollectionController.installCollectionFromRemoteRepo(cpApp.core, collectionGuid, ref returnErrorMessage, "", false, repair);
                                                        if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                                            Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                                                        }
                                                    }
                                                }
                                            } else {
                                                using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                                                    string returnErrorMessage = "";
                                                    CollectionController.installCollectionFromRemoteRepo(cpApp.core, collectionGuid, ref returnErrorMessage, "", false, repair);
                                                    if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                                        Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "-h":
                                case "--housekeep":
                                    if (!string.IsNullOrEmpty(appName)) {
                                        //
                                        // -- housekeep app
                                        using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                                            cpApp.Doc.SetProperty("force", "1");
                                            cpApp.executeAddon(Contensive.Processor.constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                                        }
                                    } else {
                                        //
                                        // -- housekeep all apps
                                        foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                                            String housekeepAppName = kvp.Key;
                                            using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(housekeepAppName)) {
                                                cpApp.Doc.SetProperty("force", "1");
                                                cpApp.executeAddon(Contensive.Processor.constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                                            }
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--version":
                                case "-v":
                                    //
                                    // display core version
                                    Console.WriteLine("version " + cpServer.core.codeVersion());
                                    exitArgumentProcessing = true;
                                    break;
                                case "--newapp":
                                case "-n":
                                    //
                                    // -- start the new app wizard
                                    CreateAppClass createApp = new CreateAppClass();
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
                                    if (!cpServer.serverOk) {
                                        //
                                        // -- something went wrong with server initialization
                                        Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] not found or not valid. Run cc --configure");
                                    } else {
                                        Console.WriteLine("Configuration File [c:\\ProgramData\\Contensive\\config.json] found.");
                                        Console.WriteLine("ServerGroup name: " + cpServer.core.serverConfig.name);
                                        Console.WriteLine("Cache: ");
                                        Console.WriteLine("    enableLocalMemoryCache: " + cpServer.core.serverConfig.enableLocalMemoryCache);
                                        Console.WriteLine("    enableLocalFileCache: " + cpServer.core.serverConfig.enableLocalFileCache);
                                        Console.WriteLine("    enableRemoteCache: " + cpServer.core.serverConfig.enableRemoteCache);
                                        Console.WriteLine("    ElastiCacheConfigurationEndpoint: " + cpServer.core.serverConfig.awsElastiCacheConfigurationEndpoint);
                                        Console.WriteLine("File System:");
                                        Console.WriteLine("    isLocal: " + cpServer.core.serverConfig.isLocalFileSystem.ToString());
                                        //Console.WriteLine("    cdnFilesRemoteEndpoint: " + cp.core.serverConfig.cdnFilesRemoteEndpoint);
                                        Console.WriteLine("    awsBucketRegionName: " + cpServer.core.serverConfig.awsBucketRegionName);
                                        Console.WriteLine("    awsBucketName: " + cpServer.core.serverConfig.awsBucketName);
                                        Console.WriteLine("    awsAccessKey: " + cpServer.core.serverConfig.awsAccessKey);
                                        Console.WriteLine("    awsSecretAccessKey: " + cpServer.core.serverConfig.awsSecretAccessKey);
                                        Console.WriteLine("Database:");
                                        Console.WriteLine("    defaultDataSourceAddress: " + cpServer.core.serverConfig.defaultDataSourceAddress.ToString());
                                        Console.WriteLine("    defaultDataSourceType: " + cpServer.core.serverConfig.defaultDataSourceType.ToString());
                                        Console.WriteLine("    defaultDataSourceUsername: " + cpServer.core.serverConfig.defaultDataSourceUsername.ToString());
                                        Console.WriteLine("Services:");
                                        Console.WriteLine("    TaskScheduler: " + cpServer.core.serverConfig.allowTaskSchedulerService.ToString());
                                        Console.WriteLine("    TaskRunner: " + cpServer.core.serverConfig.allowTaskRunnerService.ToString());
                                        Console.WriteLine("    TaskRunner-MaxConcurrentTasksPerServer: " + cpServer.core.serverConfig.maxConcurrentTasksPerServer.ToString());
                                        Console.WriteLine("Logging:");
                                        Console.WriteLine("    enableLogging: " + cpServer.core.serverConfig.enableLogging.ToString());
                                        Console.WriteLine("Applications: " + cpServer.core.serverConfig.apps.Count);
                                        foreach (KeyValuePair<string, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                                            AppConfigModel app = kvp.Value;
                                            Console.WriteLine("    name: " + app.name);
                                            Console.WriteLine("        enabled: " + app.enabled);
                                            Console.WriteLine("        admin route: " + app.adminRoute);
                                            Console.WriteLine("        local file storage");
                                            Console.WriteLine("            app path: " + app.localWwwPath);
                                            Console.WriteLine("            private path: " + app.localPrivatePath);
                                            Console.WriteLine("            cdn path: " + app.localFilesPath);
                                            Console.WriteLine("            temp path: " + app.localTempPath);
                                            if (!cpServer.core.serverConfig.isLocalFileSystem) {
                                                Console.WriteLine("        remote file storage");
                                                Console.WriteLine("            app path: " + app.remoteWwwPath);
                                                Console.WriteLine("            private path: " + app.remotePrivatePath);
                                                Console.WriteLine("            cdn path: " + app.remoteFilePath);
                                            }
                                            Console.WriteLine("        cdnFilesNetprefix: " + app.cdnFileUrl);
                                            foreach (string domain in app.domainList) {
                                                Console.WriteLine("        domain: " + domain);
                                            }
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--repair":
                                case "-r":
                                    upgrade(cpServer, appName, true);
                                    exitArgumentProcessing = true;
                                    break;
                                case "--upgrade":
                                case "-u":
                                    upgrade(cpServer, appName, false);
                                    exitArgumentProcessing = true;
                                    break;
                                case "--taskscheduler":
                                    //
                                    // -- manage the task scheduler
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run") {
                                            //
                                            // run the taskscheduler in the console
                                            Console.WriteLine("Beginning command line taskScheduler. Hit any key to exit");
                                            TaskSchedulerControllerx taskScheduler = new TaskSchedulerControllerx();
                                            taskScheduler.startTimerEvents();
                                            object keyStroke = Console.ReadKey();
                                            taskScheduler.stopTimerEvents();
                                            exitArgumentProcessing = true;
                                        } else {
                                            //
                                            // turn the windows service scheduler on/off
                                            cpServer.core.serverConfig.allowTaskSchedulerService = Contensive.Processor.Controllers.GenericController.encodeBoolean(args[argPtr]);
                                            cpServer.core.serverConfig.saveObject(cpServer.core);
                                            Console.WriteLine("allowtaskscheduler set " + cpServer.core.serverConfig.allowTaskSchedulerService.ToString());
                                        }
                                    }
                                    break;
                                case "--taskrunner":
                                    //
                                    // -- manager the task runner
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run") {
                                            //
                                            // -- run the taskrunner in the console
                                            Console.WriteLine("Beginning command line taskRunner. Hit any key to exit");
                                            using (TaskRunnerController taskRunner = new TaskRunnerController()) {
                                                taskRunner.startTimerEvents();
                                                object keyStroke = Console.ReadKey();
                                                taskRunner.stopTimerEvents();
                                                exitArgumentProcessing = true;
                                            }
                                        } else {
                                            //
                                            // -- turn the windows service scheduler on/off
                                            cpServer.core.serverConfig.allowTaskRunnerService = Contensive.Processor.Controllers.GenericController.encodeBoolean(args[argPtr]);
                                            cpServer.core.serverConfig.saveObject(cpServer.core);
                                            Console.WriteLine("allowtaskrunner set " + cpServer.core.serverConfig.allowTaskRunnerService.ToString());
                                        }
                                    }
                                    break;
                                case "--tasks":
                                    //
                                    // -- turn on, off or run both services together
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        if (args[argPtr].ToLower() == "run") {
                                            //
                                            // run the tasks in the console
                                            //
                                            Console.WriteLine("Beginning command line taskScheduler and taskRunner. Hit any key to exit");
                                            //
                                            TaskSchedulerControllerx taskScheduler = new TaskSchedulerControllerx();
                                            taskScheduler.startTimerEvents();
                                            //
                                            using (TaskRunnerController taskRunner = new TaskRunnerController()) {
                                                taskRunner.startTimerEvents();
                                                object keyStroke = Console.ReadKey();
                                                taskRunner.stopTimerEvents();
                                            }
                                            exitArgumentProcessing = true;
                                        } else {
                                            //
                                            // turn the windows service scheduler on/off
                                            //
                                            cpServer.core.serverConfig.allowTaskSchedulerService = Contensive.Processor.Controllers.GenericController.encodeBoolean(args[argPtr]);
                                            cpServer.core.serverConfig.allowTaskRunnerService = Contensive.Processor.Controllers.GenericController.encodeBoolean(args[argPtr]);
                                            cpServer.core.serverConfig.saveObject(cpServer.core);
                                            Console.WriteLine("allowTaskScheduler set " + cpServer.core.serverConfig.allowTaskSchedulerService.ToString());
                                            Console.WriteLine("allowTaskRunner set " + cpServer.core.serverConfig.allowTaskRunnerService.ToString());
                                        }
                                    }
                                    break;
                                case "--logging":
                                    //
                                    // -- logging
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        cpServer.core.serverConfig.enableLogging = GenericController.encodeBoolean(args[argPtr].ToLower());
                                        cpServer.core.serverConfig.saveObject(cpServer.core);
                                        Console.WriteLine("enableLogging set " + cpServer.core.serverConfig.enableLogging.ToString());
                                    }
                                    break;
                                case "--runtask":
                                    //
                                    // -- run task in ccTasks table in application appName 
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        appName = args[argPtr];
                                        string runnerGuid = "";
                                        if (argPtr < (args.Length + 1)) {
                                            argPtr++;
                                            runnerGuid = args[argPtr];
                                        }
                                        Console.WriteLine("runTask, appName [" + appName + "], runnerGuid [" + runnerGuid + "]");
                                        TaskRunnerController.runTask(appName, runnerGuid);
                                    }
                                    break;
                                case "--installservice":
                                    string installService = "/C C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\installutil TaskService.exe";
                                    System.Diagnostics.Process.Start("cmd.exe", installService);
                                    break;
                                case "--uninstallservice":
                                    string unInstallService = "/C C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\installutil /u TaskService.exe";
                                    System.Diagnostics.Process.Start("cmd.exe", unInstallService);
                                    break;
                                //
                                // -- run task in ccTasks table in application appName 
                                default:
                                    Console.Write(helpText);
                                    exitArgumentProcessing = true;
                                    break;
                            }
                            if (exitArgumentProcessing) break;
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("There was an error that forced the program to close. Details follow.\n\n" + ex.ToString());
            }
        }
        //
        const string helpText = ""
            + "\r\ncc command line"
            + "\r\n"
            + "\r\n-a appName"
            + "\r\n    apply the current command to just one application"
            + "\r\n"
            + "\r\n--configure"
            + "\r\n    setup or review server configuration (Sql, cache, filesystem, etc)"
            + "\r\n"
            + "\r\n--flushcache"
            + "\r\n    invalidate cache. Use with -a"
            + "\r\n"
            + "\r\n--housekeep (-h)"
            + "\r\n    housekeep all appications, or just one if specifid with -a"
            + "\r\n"
            + "\r\n--logging true|false"
            + "\r\n    Enable or disable logging at the server level"
            + "\r\n"
            + "\r\n--newapp (-n)"
            + "\r\n    new application wizard"
            + "\r\n"
            + "\r\n--repair (-r)"
            + "\r\n    reinstall the base collection and all it's dependancies. For all applications, or just one if specified with -a"
            + "\r\n"
            + "\r\n--status (-s)"
            + "\r\n    display configuration status"
            + "\r\n"
            + "\r\n--taskrunner run"
            + "\r\n    Run the taskrunner in the console (temporary)"
            + "\r\n"
            + "\r\n--taskrunner on|off"
            + "\r\n    Start or stop the taskrunner service"
            + "\r\n"
            + "\r\n--tasks run"
            + "\r\n    Run the taskscheduler and the taskrunner in the console (temporary)"
            + "\r\n"
            + "\r\n--taskscheduler run"
            + "\r\n    Run the taskscheduler in the console (temporary)"
            + "\r\n"
            + "\r\n--taskscheduler on|off"
            + "\r\n    Start or stop the taskscheduler service"
            + "\r\n"
            + "\r\n--upgrade (-u)"
            + "\r\n    upgrade all applications, or just one if specified with -a"
            + "\r\n"
            + "\r\n--version (-v)"
            + "\r\n    display code version"
            + "\r\n"
            + "\r\n--runtask appName {taskGuid}"
            + "\r\n    executes the ccTask table record with cmdRunner=(taskGuid)"
            + "\r\n"
            + "\r\n--install CollectionName"
            + "\r\n    downloads and installed the addon collection named from the Contensive Support Library"
            + "\r\n"
            + "\r\n--installservice"
            + "\r\n    Installs the TaskService.exe to Windows Services"
            + "\r\n"
            + "\r\n--uninstallservice"
            + "\r\n    Uninstalls the TaskService.exe from Windows Services"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// Upgrade a single or all apps, optionally forcing full install to include up-to-date collections (to fix broken collection addons)
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="repair"></param>
        private static void upgrade( Contensive.Processor.CPClass cp, string appName, bool repair) {
            if (!string.IsNullOrEmpty(appName)) {
                //
                // -- upgrade app
                using (Contensive.Processor.CPClass upgradeApp = new Contensive.Processor.CPClass(appName)) {
                    Processor.Controllers.AppBuilderController.upgrade(upgradeApp.core, false, repair);
                }
            } else {
                //
                // -- upgrade all apps
                foreach (KeyValuePair<String, AppConfigModel> kvp in cp.core.serverConfig.apps) {
                    using (Contensive.Processor.CPClass upgradeApp = new Contensive.Processor.CPClass(kvp.Key)) {
                        Processor.Controllers.AppBuilderController.upgrade(upgradeApp.core, false, repair);
                    }
                }
            }
        }
    }

}
