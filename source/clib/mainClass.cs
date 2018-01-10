
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Contensive.Core.Controllers;
using Contensive.Core.Models.Context;

namespace Contensive.CLI {
    class mainClass {
        static void Main(string[] args) {
            try {
                string appName = "";
                string collectionName = "";
                //
                // create cp for cluster work, with no application
                //
                //System.IO.File.WriteAllText("c:\\tmp\\clib.log", String.Join(",",args) );
                if (args.Length == 0) {
                    Console.WriteLine(helpText); // Check for null array
                } else {
                    //
                    // -- create an instance of cp to execute commands
                    using (Core.CPClass cp = new Core.CPClass()) {
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
                        if (String.IsNullOrEmpty(cp.core.serverConfig.programFilesPath)) {
                            string executePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                            if (executePath.ToLower().IndexOf("\\git\\") == 0) {
                                //  -- save if not in developer execution path
                                cp.core.serverConfig.programFilesPath = executePath;
                            } else {
                                //  -- developer, fake a path
                                cp.core.serverConfig.programFilesPath = "c:\\Program Files (x86)\\kma\\Contensive5\\";
                            }
                            cp.core.serverConfig.saveObject(cp.core);
                        }
                        //
                        // -- loop through arguments and execute each command
                        for (int argPtr = 0; argPtr < args.Length; argPtr++) {
                            string argument = args[argPtr];
                            bool exitArgumentProcessing = false;
                            switch (argument.ToLower()) {
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
                                        var collectionList = new List<Contensive.Core.collectionController.collectionStoreClass>();
                                        Contensive.Core.collectionController.getRemoteCollectionList(cp.core, ref collectionList);
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
                                                foreach (KeyValuePair<String, serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps) {
                                                    using (Contensive.Core.CPClass cpApp = new Contensive.Core.CPClass(kvp.Key)) {
                                                        string returnErrorMessage = "";
                                                        Contensive.Core.collectionController.installCollectionFromRemoteRepo(cpApp.core, collectionGuid, ref returnErrorMessage, "", false);
                                                        if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                                            Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                                                        }
                                                    }
                                                }
                                            } else {
                                                using (Contensive.Core.CPClass cpApp = new Contensive.Core.CPClass(appName)) {
                                                    string returnErrorMessage = "";
                                                    Contensive.Core.collectionController.installCollectionFromRemoteRepo(cpApp.core, collectionGuid, ref returnErrorMessage, "", false);
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
                                        using (Contensive.Core.CPClass cpApp = new Contensive.Core.CPClass(appName)) {
                                            cpApp.Doc.SetProperty("force", "1");
                                            cpApp.executeAddon(Contensive.Core.constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                                        }
                                    } else {
                                        //
                                        // -- housekeep all apps
                                        foreach (KeyValuePair<String, serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps) {
                                            String upgradeAppName = kvp.Key;
                                            using (Contensive.Core.CPClass cpApp = new Contensive.Core.CPClass(upgradeAppName)) {
                                                cpApp.Doc.SetProperty("force", "1");
                                                cpApp.executeAddon(Contensive.Core.constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                                            }
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--version":
                                case "-v":
                                    //
                                    // display core version
                                    Console.WriteLine("version " + cp.core.codeVersion());
                                    exitArgumentProcessing = true;
                                    break;
                                case "--newapp":
                                case "-n":
                                    //
                                    // -- start the new app wizard
                                    createAppClass createApp = new createAppClass();
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
                                    if (!cp.serverOk) {
                                        //
                                        // -- something went wrong with server initialization
                                        Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] not found or not valid. Run clib --configure");
                                    } else {
                                        Console.WriteLine("Configuration File [c:\\ProgramData\\Contensive\\config.json] found.");
                                        Console.WriteLine("ServerGroup name: " + cp.core.serverConfig.name);
                                        Console.WriteLine("Cache: ");
                                        Console.WriteLine("    enableLocalMemoryCache: " + cp.core.serverConfig.enableLocalMemoryCache);
                                        Console.WriteLine("    enableLocalFileCache: " + cp.core.serverConfig.enableLocalFileCache);
                                        Console.WriteLine("    enableRemoteCache: " + cp.core.serverConfig.enableRemoteCache);
                                        Console.WriteLine("    ElastiCacheConfigurationEndpoint: " + cp.core.serverConfig.awsElastiCacheConfigurationEndpoint);
                                        Console.WriteLine("File System:");
                                        Console.WriteLine("    isLocal: " + cp.core.serverConfig.isLocalFileSystem.ToString());
                                        Console.WriteLine("    cdnFilesRemoteEndpoint: " + cp.core.serverConfig.cdnFilesRemoteEndpoint);
                                        Console.WriteLine("Database:");
                                        Console.WriteLine("    defaultDataSourceAddress: " + cp.core.serverConfig.defaultDataSourceAddress.ToString());
                                        Console.WriteLine("    defaultDataSourceType: " + cp.core.serverConfig.defaultDataSourceType.ToString());
                                        Console.WriteLine("    defaultDataSourceUsername: " + cp.core.serverConfig.defaultDataSourceUsername.ToString());
                                        Console.WriteLine("Services:");
                                        Console.WriteLine("    TaskScheduler: " + cp.core.serverConfig.allowTaskSchedulerService.ToString());
                                        Console.WriteLine("    TaskRunner: " + cp.core.serverConfig.allowTaskRunnerService.ToString());
                                        Console.WriteLine("    TaskRunner-MaxConcurrentTasksPerServer: " + cp.core.serverConfig.maxConcurrentTasksPerServer.ToString());
                                        Console.WriteLine("Logging:");
                                        Console.WriteLine("    enableLogging: " + cp.core.serverConfig.enableLogging.ToString());
                                        Console.WriteLine("Applications: " + cp.core.serverConfig.apps.Count);
                                        foreach (KeyValuePair<string, serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps) {
                                            serverConfigModel.appConfigModel app = kvp.Value;
                                            Console.WriteLine("    name: " + app.name);
                                            Console.WriteLine("        enabled: " + app.enabled);
                                            Console.WriteLine("        adminRoute: " + app.adminRoute);
                                            Console.WriteLine("        appRootPath: " + app.appRootFilesPath);
                                            Console.WriteLine("        privateFilesPath: " + app.privateFilesPath);
                                            Console.WriteLine("        cdnFilesPath: " + app.cdnFilesPath);
                                            Console.WriteLine("        cdnFilesNetprefix: " + app.cdnFilesNetprefix);
                                            foreach (string domain in app.domainList) {
                                                Console.WriteLine("        domain: " + domain);
                                            }
                                        }
                                    }
                                    exitArgumentProcessing = true;
                                    break;
                                case "--upgrade":
                                case "-u":
                                    if (!string.IsNullOrEmpty(appName)) {
                                        //
                                        // -- upgrade app
                                        using (Contensive.Core.CPClass upgradeApp = new Contensive.Core.CPClass(appName)) {
                                            Core.Controllers.appBuilderController.upgrade(upgradeApp.core, false);
                                        }
                                    } else {
                                        //
                                        // -- upgrade all apps
                                        foreach (KeyValuePair<String, serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps) {
                                            using (Contensive.Core.CPClass upgradeApp = new Contensive.Core.CPClass(kvp.Key)) {
                                                Core.Controllers.appBuilderController.upgrade(upgradeApp.core, false);
                                            }
                                        }
                                    }
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
                                            taskSchedulerController taskScheduler = new taskSchedulerController();
                                            taskScheduler.allowVerboseLogging = true;
                                            taskScheduler.allowConsoleWrite = true;
                                            taskScheduler.startTimerEvents(true, false);
                                            object keyStroke = Console.ReadKey();
                                            taskScheduler.stopTimerEvents();
                                            exitArgumentProcessing = true;
                                        } else {
                                            //
                                            // turn the windows service scheduler on/off
                                            cp.core.serverConfig.allowTaskSchedulerService = Contensive.Core.Controllers.genericController.encodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowtaskscheduler set " + cp.core.serverConfig.allowTaskSchedulerService.ToString());
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
                                            taskRunnerController taskRunner = new taskRunnerController();
                                            taskRunner.startTimerEvents();
                                            object keyStroke = Console.ReadKey();
                                            taskRunner.stopTimerEvents();
                                            exitArgumentProcessing = true;
                                        } else {
                                            //
                                            // -- turn the windows service scheduler on/off
                                            cp.core.serverConfig.allowTaskRunnerService = Contensive.Core.Controllers.genericController.encodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowtaskrunner set " + cp.core.serverConfig.allowTaskRunnerService.ToString());
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
                                            taskSchedulerController taskScheduler = new taskSchedulerController();
                                            taskScheduler.allowVerboseLogging = true;
                                            taskScheduler.allowConsoleWrite = true;
                                            taskScheduler.startTimerEvents(true, false);
                                            //
                                            taskRunnerController taskRunner = new taskRunnerController();
                                            taskRunner.startTimerEvents();
                                            //
                                            object keyStroke = Console.ReadKey();
                                            taskRunner.stopTimerEvents();
                                            exitArgumentProcessing = true;
                                        } else {
                                            //
                                            // turn the windows service scheduler on/off
                                            //
                                            cp.core.serverConfig.allowTaskSchedulerService = Contensive.Core.Controllers.genericController.encodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.allowTaskRunnerService = Contensive.Core.Controllers.genericController.encodeBoolean(args[argPtr]);
                                            cp.core.serverConfig.saveObject(cp.core);
                                            Console.WriteLine("allowTaskScheduler set " + cp.core.serverConfig.allowTaskSchedulerService.ToString());
                                            Console.WriteLine("allowTaskRunner set " + cp.core.serverConfig.allowTaskRunnerService.ToString());
                                        }
                                    }
                                    break;
                                case "--enablelogging":
                                    //
                                    // -- logging
                                    if (argPtr != (args.Length + 1)) {
                                        argPtr++;
                                        cp.core.serverConfig.enableLogging = genericController.encodeBoolean(args[argPtr].ToLower());
                                        cp.core.serverConfig.saveObject(cp.core);
                                        Console.WriteLine("enableLogging set " + cp.core.serverConfig.enableLogging.ToString());
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
            } catch (Exception ex) {
                Console.WriteLine("There was an error that forced the program to close. Details follow.\n\n" + ex.ToString());
            }
        }
        //
        const string helpText = ""
            + "\r\nclib command line"
            + "\r\n"
            + "\r\n--newapp (-n)"
            + "\r\n    new application wizard"
            + "\r\n"
            + "\r\n--upgrade appName (-u appname)"
            + "\r\n    run application upgrade"
            + "\r\n"
            + "\r\n--upgradeall"
            + "\r\n    run application upgrade on all applications"
            + "\r\n"
            + "\r\n--housekeep appName (-h appname)"
            + "\r\n    run application housekeeping"
            + "\r\n"
            + "\r\n--housekeepall"
            + "\r\n    run application housekeeping on all applications"
            + "\r\n"
            + "\r\n--version (-v)"
            + "\r\n    display code version"
            + "\r\n"
            + "\r\n--status (-s)"
            + "\r\n    display configuration status"
            + "\r\n"
            + "\r\n--taskscheduler run"
            + "\r\n    Run the taskscheduler in the console (temporary)"
            + "\r\n"
            + "\r\n--enablelogging true|false"
            + "\r\n    Enable or disable logging at the server level"
            + "\r\n"
            + "\r\n--taskscheduler on|off"
            + "\r\n    Start or stop the taskscheduler service"
            + "\r\n"
            + "\r\n--taskrunner run"
            + "\r\n    Run the taskrunner in the console (temporary)"
            + "\r\n"
            + "\r\n--taskrunner on|off"
            + "\r\n    Start or stop the taskrunner service"
            + "\r\n"
            + "\r\n--tasks run"
            + "\r\n    Run the taskscheduler and the taskrunner in the console (temporary)"
            + "";
    }

}
