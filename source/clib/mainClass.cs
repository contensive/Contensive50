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
                CPClass cp;
                CPClass cpServerGroup;
                coreFileSystemClass installFiles;
                coreFileSystemClass programDataFiles;
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
                    //
                    // start by creating an application event log entry - because you must be admin to make this entry so making it here will create the "source"
                    //
                    string sSource = "Contensive";
                    string sLog = "Application";
                    string sEvent = "command line:";
                    for (int i = 0; i < args.Length; i++)
                    {
                        sEvent += " " + args[i];
                    }
                    if (!EventLog.SourceExists(sSource)) EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Information);
                    //
                    bool exitCmd = false;
                    for (int i = 0; i < args.Length; i++) // Loop through array
                    {
                        createAppClass createApp = new createAppClass();
                        string argument = args[i];
                        switch (argument.ToLower())
                        {
                            case "--version":
                            case "-v":
                                //
                                // display core version
                                //
                                cp = new CPClass();
                                Console.WriteLine("version " + cp.core.common_version());
                                exitCmd = true;
                                cp.Dispose();
                                break;
                            case "--newapp":
                            case "-n":
                                //
                                // start the new app wizard
                                //
                                createApp.createApp();
                                exitCmd = true;
                                break;
                            case "--configure":
                                //
                                // eventually write a configure. For now, just use the new app
                                //
                                configureClass.configure();
                                exitCmd = true;
                                break;
                            case "--status":
                            case "-s":
                                //
                                // display ServerGroup and application status
                                //
                                cp = new CPClass();
                                //
                                if (!cp.serverOk)
                                {
                                    //c:\\programData\\clib\\serverconfig.json
                                    Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] not found or not valid. Run clib --configure");
                                }
                                else
                                {
                                    Console.WriteLine("configuration file [c:\\ProgramData\\Contensive\\config.json] found.");
                                    Console.WriteLine("ServerGroup name: " + cp.core.serverConfig.name);
                                    Console.WriteLine("appPattern: " + cp.core.serverConfig.appPattern);
                                    Console.WriteLine("ElastiCacheConfigurationEndpoint: " + cp.core.serverConfig.awsElastiCacheConfigurationEndpoint);
                                    Console.WriteLine("cdnFilesRemoteEndpoint: " + cp.core.serverConfig.cdnFilesRemoteEndpoint);
                                    Console.WriteLine("isLocal: " + cp.core.serverConfig.isLocalFileSystem.ToString());
                                    //Console.WriteLine("serverPhysicalPath: " + cp.core.serverConfig.serverPath.ToString());
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

                                exitCmd = true;
                                break;
                            case "--upgrade":
                            case "-u":
                                //
                                // upgrade the app in the argument list, or prompt for it
                                //
                                if (i == (args.Length + 1))
                                {
                                    Console.WriteLine("Application name?");
                                    appName = Console.ReadLine();
                                }
                                else {
                                    i++;
                                    appName = args[i];
                                }
                                if (string.IsNullOrEmpty(appName))
                                {
                                    Console.WriteLine("ERROR: upgrade requires a valid app name.");
                                    i = args.Length;
                                }
                                else {
                                    cp = new CPClass(appName);
                                    installFiles = new coreFileSystemClass(cp.core, cp.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                                    coreBuilderClass builder = new coreBuilderClass(cp.core);
                                    builder.upgrade(false);
                                    installFiles.Dispose();
                                    cp.Dispose();
                                }
                                exitCmd = true;
                                break;
                            case "--upgradeall":
                                //
                                // upgrade all apps in the server group
                                //
                                using (cpServerGroup = new CPClass())
                                {
                                    using (installFiles = new coreFileSystemClass(cpServerGroup.core, cpServerGroup.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)))
                                    {
                                        Console.WriteLine("Upgrading cluster folder clibResources from installation");
                                        //
                                        foreach (var item in cpServerGroup.core.serverConfig.apps)
                                        {
                                            cp = new CPClass(item.Key);
                                            coreBuilderClass builder = new coreBuilderClass(cp.core);
                                            builder.upgrade(false);
                                        }
                                        exitCmd = true;
                                    }
                                }
                                break;
                            case "--taskscheduler":
                                using (cpServerGroup = new CPClass())
                                {
                                    using (programDataFiles = new coreFileSystemClass(cpServerGroup.core, cpServerGroup.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib"))
                                    {
                                        JSONTemp = programDataFiles.readFile("serverConfig.json");
                                        if (string.IsNullOrEmpty(JSONTemp))
                                        {
                                            Console.WriteLine("The serverConfig.json file was not found in c:\\programData\\clib. Please run -n to initialize the server.");
                                        }
                                        else
                                        {
                                            //System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                            Models.Entity.serverConfigModel serverConfig = cpServerGroup.core.json.Deserialize<Models.Entity.serverConfigModel>(JSONTemp);
                                            if (i != (args.Length + 1))
                                            {
                                                i++;
                                                if (args[i].ToLower() == "run")
                                                {
                                                    //
                                                    // run the taskscheduler in the console
                                                    //
                                                    Console.WriteLine("Beginning command line taskScheduler. Hit any key to exit");
                                                    coreTaskSchedulerServiceClass taskScheduler = new coreTaskSchedulerServiceClass();
                                                    taskScheduler.allowVerboseLogging = true;
                                                    taskScheduler.allowConsoleWrite = true;
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
                                                    serverConfig.allowTaskSchedulerService = cpServerGroup.Utils.EncodeBoolean(args[i]);
                                                    Console.WriteLine("allowtaskscheduler set " + serverConfig.allowTaskSchedulerService.ToString());
                                                    programDataFiles.saveFile("serverConfig.json", cpServerGroup.core.json.Serialize(serverConfig));
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case "--taskrunner":
                                using (cpServerGroup = new CPClass())
                                {
                                    using (programDataFiles = new coreFileSystemClass(cpServerGroup.core, cpServerGroup.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib"))
                                    {
                                        JSONTemp = programDataFiles.readFile("serverConfig.json");
                                        if (string.IsNullOrEmpty(JSONTemp))
                                        {
                                            Console.WriteLine("The serverConfig.json file was no found in c:\\programData\\clib. Please run -n to initialize the server.");
                                        }
                                        else
                                        {
                                            //System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                            Models.Entity.serverConfigModel serverConfig = cpServerGroup.core.json.Deserialize<Models.Entity.serverConfigModel>(JSONTemp);
                                            if (i != (args.Length + 1))
                                            {
                                                i++;
                                                if (args[i].ToLower() == "run")
                                                {
                                                    //
                                                    // run the taskrunner in the console
                                                    //
                                                    Console.WriteLine("Beginning command line taskRunner. Hit any key to exit");
                                                    coreTaskRunnerServiceClass taskRunner = new coreTaskRunnerServiceClass();
                                                    taskRunner.allowVerboseLogging = true;
                                                    taskRunner.allowConsoleWrite = true;
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
                                                    serverConfig.allowTaskRunnerService = cpServerGroup.Utils.EncodeBoolean(args[i]);
                                                    Console.WriteLine("allowtaskrunner set " + serverConfig.allowTaskRunnerService.ToString());
                                                    programDataFiles.saveFile("serverConfig.json", cpServerGroup.core.json.Serialize(serverConfig));
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case "--tasks":
                                //
                                // turn on, off or run both services together
                                //
                                using (cpServerGroup = new CPClass())
                                {
                                    using (programDataFiles = new coreFileSystemClass(cpServerGroup.core, cpServerGroup.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib"))
                                    {
                                        cpServerGroup = new CPClass();
                                        programDataFiles = new coreFileSystemClass(cpServerGroup.core, cpServerGroup.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib");
                                        JSONTemp = programDataFiles.readFile("serverConfig.json");
                                        if (string.IsNullOrEmpty(JSONTemp))
                                        {
                                            Console.WriteLine("The serverConfig.json file was no found in c:\\programData\\clib. Please run -n to initialize the server.");
                                        }
                                        else
                                        {
                                            //System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                            Models.Entity.serverConfigModel serverConfig = cpServerGroup.core.json.Deserialize<Models.Entity.serverConfigModel>(JSONTemp);
                                            if (i != (args.Length + 1))
                                            {
                                                i++;
                                                if (args[i].ToLower() == "run")
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
                                                    exitCmd = true;
                                                }
                                                else
                                                {
                                                    //
                                                    // turn the windows service scheduler on/off
                                                    //
                                                    serverConfig.allowTaskSchedulerService = cpServerGroup.Utils.EncodeBoolean(args[i]);
                                                    Console.WriteLine("allowTaskScheduler set " + serverConfig.allowTaskSchedulerService.ToString());
                                                    //
                                                    serverConfig.allowTaskRunnerService = cpServerGroup.Utils.EncodeBoolean(args[i]);
                                                    Console.WriteLine("allowTaskRunner set " + serverConfig.allowTaskRunnerService.ToString());
                                                    //
                                                    programDataFiles.saveFile("serverConfig.json", cpServerGroup.core.json.Serialize(serverConfig));
                                                }
                                            }
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
