
updated: 20161020

debug
---- all bug logs in github

Why do this / use this
---- use successful development patterns -- internally we all develop the same way, and that way is compatible with all the libraries we have written so far.
---- reusable code -- library that minimizes errors
---- keep it simple -- keep the code from getting in the way of the developer (front or back-end)
---- it creates a meta layer to automtically create access
---- it is a layer to abstract basic io to make updating and lifecycle management easier. As technology changes, we integrate at the api layer.

Short Term Goals: 20160930
---- process addons
-------- save to task table with serialized doc properties
-------- create service, polls task table each minute and runs addon if found
---- Write "wiki" - that thing -someone- said means you have established software
/------- clib.io is the domain
-------- find a sample software project like this that I want to model after (to start, simple clean styles)
-------- write How-To-Get-Started
---- Installations
/-------- nuget package that a developer can add to webproject, develop and push to the appRoot folder to develop any type of ASPX site
/------------ long term - https://docs.nuget.org/ndocs/create-packages/creating-a-package
/------------ publishing - https://docs.nuget.org/ndocs/create-packages/publish-a-package
/------------ quickstart - 
-------- setup install
------------ Installs all software for all server types, etc and clib.exe to manage server configuration
---- configuration
-------- each server (webRole,workerRole,scheduler) is part of one and only one cluster
-------- each cluster can have one or more apps
-------- all apps in a cluster trust each other - no work is done to protect files,db,etc. from another
-------- the apps on a webRole are the same throughout the cluster
-------- the clusterConfig.json file
------------ localMode: stored in clusterFolder (d:\inetpub)
------------ scaleMode: retreived from an http source (s3) and local copy in localMode store.
------------ first node is clusterConfig source endpoint(s) that hold the authoritative copy of the clusterConfig.json
------------ has all the configuration for the cluster and for all the apps in the cluster
------------ to update it, all applications should check it's modification time and call for a refresh if > ?1 minute old
---- File Locations
-------- Program Files\clib (programFilesFolder)
------------ .\resources - files used for buildig new sites (might move to program files)
------------ cli.exe
------------ clibService.exe, windows service not installed automatically, install with 'installUtil clibService.exe'
------------ DLLs needed for exes
------------ no need to backup, no app data
-------- ProgramData\clib (programDataFolder)
------------ .\commonAssemblies, installed empty.
---------------- developers can copy addon assemblies here. Add addon assembly execution tries this first. Keep it empty in production
------------ .\logs
------------ config.json, cluster configuration file
------------ if missing, cli creates it
-------- d:\inetpub (clusterFolder)
------------ where all application data is stored (this folder is backed up)
------------ initialization sets these folders up here. It can be repointer.
------------ read/write files with cp.core.cluster.files Object
------------ cluserConfig.json - in scale mode, this is a mirror of the cluster data retrieved from and stored to an S3 bucket somewhere
-------- d:\inetPub\apps - all applications are setup here.
------------ initialization sets these folders up here. It can be repointer. all app folders are relative to this.
-------- d:\inetPub\apps\appName - application folder, these folders do not need to be here, this is the default location
------------ initialization sets these folders up here. It can be repointer. all app folders are relative to this.
------------ \appRoot - the iis root folder for the application, NOT dynamic data. This is the published documents and is NOT synchonized between webRoles
---------------- local mode, simple local file store
---------------- scale mode, simple local file store, not shared
---------------- read/write files with cp.core.cluster.app.appRootFiles Object
------------ \cdnFiles - public files like uploads (similar to 4.1 contentFiles)
---------------- local mode, simple local file st re
-------------------- for contentFiles compatibility, can be mapped into virtual iis space with appRootNetPrefix=/appName/files/
-------------------- can be mapped into virtual iis space using appRootNetPrefix=/cdnfiles/
---------------- scale mode, read initiates a file download from an s3-like server, then read. write saves then uploads file
-------------------- can be a url to an s3 store with appRootNetPrefix=http://cdn.domainName.com/
---------------- read/write files with cp.core.cluster.app.cdnFiles Object
------------ \privateFiles - files needed but do not have http exposure, local here, scale-mode this is a mirror of a cluster resource, like cdnFiles
---------------- \privateFiles\addons - addons used by the app. Originally this was a forth folder type, but it works exactly like private files
-------------------- one webRole installs a new addon, it is uploaded, Db, changed to include it. Other webRoles attempt execution, private files are read and downloaded.
---------------- local mode, simple local file store
---------------- scale mode, read initiates a file download from an s3-like server, then read. write saves then uploads file
---------------- read/write files with cp.core.cluster.app.privateFiles Object
---- To add a server to the cluster
-------- bring up a new server
-------- install clib
-------- run clibCli -addcluster http://theClusterConfigSourceEndpoint
------------ clusterConfig is downloaded, with all the appConfig
------------ clibcli creates required local folders
------------ clibcli fetches all appRoot files
------------ clibcli verifies/creates root iis site for app, pointing to appRoot
---- To create a new cluster
-------- start a new server
-------- install clib
-------- run clibcli and follow the wizard to create cluster, then first app
---- To add a new app to a cluster
-------- run clibcli on any server in the cluster
-------- clibcli prompts for appconfig data, updates clusterConfig on source, forces a local clusterConfig refresh
---- When clusterConfig refresh includes a new app
-------- creates the app locally
-------- if the server is a webRole, the webserver is updated.
---- create a stand-alone clibcli to setup/initialize the server
-------- verifies/initializes the configuration data ()
-------- verifies/initialize database (reads app.config). Progress goes to log and console.
---- /admin site like v4.1
---- /remoteMethod route execution like v4.1
---- each app has four file stores: appRoot, addons, cdnFiles, privateFiles
-------- appRoot: the physical folder hosting the application, physically on the webRole, exposted to the net on the application's domain
-------- addons: the same addon folder structure as with 4.1
-------- cdnFiles: a physical folder where uploaded content is stored, exposed to the Internet with cp.site.files (for uploaded pictures, etc)
-------- privateFiles: a physical folder where the application stores data but is not connected to the net (for logs, limited access files, etc)
---- scale mode - the cluster of apps is hosted across multiple machines
---- local mode - the cluster of apps is only hosted on one machine
---- Two basic application models: scale-up web, scale-out application
-------- scale-out app: like AMS, 
------------ domain points to cdnFiles on AWS s3 static site
------------ remotemethods points to appRoot,  only handles ajax
------------ privateFiles on second s3 store
-------- scale-up webSite: like truckload.org, 
------------ appRoot and cdnFiles both point to the iis wwwRoot
------------ privateFiles to another non-net-exposed folder on the webRole
---- Unit tests for the library written isn a stand-alone application that acts as a worker role
---- addon assemblies run from
-------- clusterfolder\clibCommonAssemblies
-------- privateFiles\addons\addonCollectionFolder\versionFolder
-------- addons in the core collection are then tried in appRoot\bin

Long Term Goal: 2017Q1
---- authentication:
-------- cookie-based authentication automatic in webRole
-------- token based authentication optional in webRole, available to workerRole (for webRole to be REST, and to avoid cookie domain issues)
---- cluster features disabled for now. Create a cluster object that requires a authKey to use, and has access to all sites.
---- integrate nlog nuget package to prevent log saving issues, and provide a standard format.

Long Term Goal: 2017Q2
---- convert to c#, then open source, then ms dotnet Core.
-------- http://www.vbconversions.com/  $199, robust
-------- http://converter.telerik.com/ free, snippet
---- convert db to mssql to use read-replicas
---- Add third application models: scale-out web
-------- scale-out webSite: appRoot is webRole,  is appRoot. All static content comes
---- consider converting c# to javascript using
-------- http://bridge.net/

Refactoring
---- convert err object to try-catch
---- rename to remove main_ and csv_ class layers and consolidate
---- move cp.ExecuteAddonAssembly to cpCore -- core should never call up to cp, it is the api
---- pageManager and adminSite are a new type of addon (?context). When done, they go in he body of an html document, constructed from addons etc.
-------- this construction is currently in pageManager and adminSite has it all hardcoded
---- executeAddon - consoldate calls, add a call to just run an addon with the current optionstring (doc properties) with a given context
---- adminSiteClass
-------- convert to remote method -- ajax method "AjaxOpenIndexFilterGetContent" calls method adminSiteClass friend GetForm_IndexFilterContent
---- refactor tools and reports into addons
-------- if they are tied in tight, put them in core collection and run from appRoot/bin
---- REFACTOR - editSourceId, etc fields only apply to page content
---- housekeep routine - should only be run by the scheduler, and make sure it goes through all the apps in the cluster
---- Option_String refactor into list or dictionary

Terms (for wiki)
---- webRole : a server that holds a webserver and executes application methods - front-end server
---- workerRole : a server that executes background tasks
---- taskScheduler : an applicatation that initiates background tasks based on a timer
---- taskRunner : 
---- testRunner :
---- cache : a 
---- Db : a 
---- cluster
---- clusterManager
---- authetication
---- appRoot
---- publicStore
---- privateStore
---- scaleUp
---- scaleOut
---- webSite
---- webApplication

Roles (for wiki)
---- developer
---- administrator
---- user
---- user-record

How-To Instructions to write
---- Quick setup a scale-out application
-------- appName must be unique in programdata, database name, iis site name
-------- Run cfw cli and setup the application (programdata folder)
-------- wwwRoot will be setup in the programdata folder. set appName in default.aspx
-------- 
-------- 
-------- 
-------- 
-------- 
-------- 
---- Add a remote method to an application
---- Quick setup a scale-up website
---- Add an addon to a website
---- Suggested scale-out application architecture (static site, dynamic app, angular, cfw)
---- Suggested scale-up website architecture (addon html/css/javascript/UI/Server considerations)


components of a running application
---- webrole - runs the application front-end
---- workerrole - executes process and scheduled addons
---- scheduler - initiates tasks (addons) on a timer to be handled by worker role(s)
---- task queue - list of tasks, added by scheduler and roles, executed by working roles
---- cache - memcached
---- Db - remote connection to SQL Server (aws rds, later migrate to MySql or other)
---- cluster - a group of applications hosted together on one or more machines.
---- cluster Source endpoint - a location that sources clusterConfig.json, appRoot and addon files

development environment
- nuget source for clib webRole references, Visual Studio can create aspx sites with clib resources.
- install program files to include clibcli
- runclibcli to setup application and connect to cluster folder



- use aspnet application config to set all application connection settings
- use visual studio publish to push changes to site.


--- not sure below this -----

todo - get a webhit to work
- siteProperties
---- step 1 - untangle the store-mode system, should do simple db get/set
-------- should not be in metaCache - should be a write-through doc-scope lazy-cache
---- step 2 - store in dictionary, onGet check exists, loadFromDb, add to dict, return
--------- onSet, update dict, Db save async?
x- cp.init() returns true/fasle based on if remoteMethods already ran, etc -- convert to cp.init() and cp.executeRoute()
- in cp.init(), context object is copied to the internal main_pageEtc. properties -- convert to have internal objects reference context directly
- iis config ?all sites on one iis site, but will SSL work? maybe one iis site per app, so instance config will ahve to include iis site setup for the who clusterAppAccess.
---- for now, assume on each server - sites use the * ip address, each app has its own site, with a root directory, and either a module or a scripting language.
---- rewrite iis setup using "http://blogs.msdn.com/b/carlosag/archive/2006/04/17/microsoftwebadministration.aspx"
- after createApp, buildversion site property="0", and there are only two
- migrate all app. methods that involve Db to a class so we attach like this app.datasource(0).executeSql and the Db type can be in the constructor, and encodeSqlDate() does not need to include a Db type.
- I think createContent4 should include isBaseContent in the update queries (it is in another section later, weird but ok)
- when opening datatables, convert to using()
- search and replace out
-- dt should all dispose
- add option strict
- work on method name consistency - like getFileInfo, pick folder or dir, etc.
- get rid of the extra layers of code on files and b, like 'main_GetFileList'

- appServicesClass loads during constructor, NOT from controlClass
----- controlClass does developer work, like upgrading apps
----- appServices loads appConfigclass internally
----- create appConfigClass within appServices to hold complete configuration
----- appConfig file is only modified by controlling application - never from a web or cmd client, never within appServicesClass
- need to add enabled to appConfig, set on strart and cleared on stop - enables webrole, allows workerroles
----- controlClass modifies appConfig and next web hit gets the setting, cmdProcessor gets it on next load
- if we need to enumerate apps, control class has getClusterAppList
----- make this friend so it can only be calledinternally
- create contentDefinitionClass to serialize and deserialize to/from appServicesClass
- get a site up and running (manually copy an existing Db and hand setup public/private paths)
----- debug issues here before continuing with a site builder
- write a cmd line app to build apps
----- appconfig files, serializing from the using the same class used internally
----- config folder, domain folder, Db, public and private files
----- first handle instance mode, then scale mode
- setup httpModule to handle hits, create cp, etc.
- move class libraries to a single core project and use 'friend' and 'private' to protect anything that should not exposed (control call)
----- then createa project for each 'deliverable', like the cmd, or the webclient, or windows service.

Contensive5 Dev plan

examples refer to same site www.acme.com, application name acme

1) Architecture
---- Focus on making it easy and fast to developer
-------- be able to easily profile the code remotely - and be able to act on performance issues
-------- apps portable : migrating an app to a different server should be easy and fast
------------ to handle when sites need to be deployed and redistributed
------------ all app files should be in one easy place
-------- continuous deployment, no big releases, but conuouosly deploy updates
-------- ALL public interfaces have testing code so the API will never deprecate unexpected, breaking charges require new methods
-------- Separate visual studio from html/css from javascript - three environments that can be controlled from three developers (THREE FOLDERS)
------------ html/css should come from a git repo or ftp and go to a source folder (s3 if possible) where it is read and integrated with a deploy process
------------ client scripting (javascript) can be in a seperate git. You can deploy as part of a collection, but develop with an dtp push or git repo copy
------------ server code (visual studio) can be in seperated git. YOu can deploy DLLs as part of a collection, but during development just copy them or get repo copy
-------- You should not have to know about everything to develop your part.
---- client - is any body of code that uses cfw
---- webRole client - a virtual machine that handles website requests
---- workerRole client - a virtual machine that handles processes not related to web hits
---- appName is unique in the cluster and never appears in the Url (so it never needs to be changed)
---- addon collection
---- addon collection deployment folder (***** NEW *****)
-------- webRole - ( wrong I think )dataDrive:/cfw/apps//acme/addons/myCollectionFiles/bin/myCollection.dll 
-------- s3 cdn - s3:cfwCluster0/acme/cdnFiles/myCollectionFiles/img/image.png
-------- s3 cdn - s3:cfwCluster0/acme/cdnFiles/myCollectionFiles/css/myStyles.css
-------- s3 cdn - s3:cfwCluster0/acme/cdnFiles/myCollectionFiles/js/backoffice.js
-------- s3 cluster - s3:cfwCluster0/acme/privateFiles/myCollection/somethingNotPublic.xml
-------- the folder name (myCollectionFiles) is a property of addon collections - the name of the physical folder within public and private files where all collection files are saved
-------- during export, these filea are zipped. During deploy this folder is created or cleared and the new files are added
-------- developers can edit files in that folder directly and they will run.
---- Functional components
-------- content is editing by admins
-------- Deploy process is used to deploy templates and addons, run each time addson change to clear addon cache
---- Data architecture
-------- dynamic content rendered by a htmlRenderMethod - html pages delivered in response to browser requests
-------- ajax requests are handled by remoteMethods - can return data in any format, not counted as pages
-------- static resources like css, images, js, uploaded content - delivered by cdn
-------- private static content (uploaded files) is part of cdn, described within each mode
-------- contentDefinitions are just a cache entry (serialized object with index objects attached)
-------- site properties, visitProperties, visitorProperties, userProperties are a writethrough cache
---- scale mode, a group of resources (machines and services) that interact to execute applications is a Cluster
-------- a cluster is a single datasource instance (with 1 to many databases) and 1 s3 bucket, can handle 1 to many sites
-------- htmlRenderMethod,  comes from an httpModule in an iis site
------------ only delivers dynamic content (html), never static resourses (css,img,js,etc)
------------ creates, configures and initializes cp, then calls an addon based on the url domain (each domain gets one)
------------ primary html interface
------------ gets access to Db, cache and S3 resources with permissions in configuration file
------------ code accesses files using api (logs, publicFile, privateFile), never directly to instance
------------ src tags in the html point to cdn (should be handled in deployment process)
-------- remoteMethod,  come from an httpModule in an iis site
------------ can be a cfw interface, or any code with access to the db, files
---------------- Adapters - api for other platforms to access db,files. Initialize cp then share config info
---------------- write adapters for node,php
------------ only delivers dynamic content (json,text,html), never static resourses (css,img,js,etc)
------------ creates, configures and initializes cp, then calls the remote method based on url path
------------ gets access to Db, cache and S3 resources with permissions in configuration file
------------ only writes files using api (logs, publicFile, privateFile), never directly to instance
------------ src tags in the html are altered during rendering
-------- cdn is a folder (/acme/cdnFiles) in the cluster's s3 bucket. Each app gets a folder mapped to the the cdn domain
------------ for https or more performance, a cloudfront interface can be used in front of the s3 bucket
-------- private static content is a folder (/acme/privateFiles) in the s3 cluster bucket.
------------ access is granted through aws time-limited urls
------------ files to be downloaded will be granted temporary credentials by the source page.
------------ if text buffering files are stored here, they are delivered indirectly from the htmlRenderMethod or remoteMethod
------------ apps addons are stored here, and downloaded to a local cache. Local cache cleared on Deploy
-------- configuration files
------------ clusterConfig.json - installs in \ProgramData\cfw with HtmlRenderMethod and remoteMethods
---------------- points to cluster's s3 bucket, where the appConfig and appDomain Lists
------------ appConfig files - an s3 folder in the cluster bucket (/appConfig) with application config files, named (mySite.json)
---------------- contains endpoints and credentials to access site resources like db, cache, s3 private and public folders
---------------- these are settings not editable online because mistakes would make the application unusable to fix the mistake
---------------- cache backed by file download and open
------------ domainConfig files - an s3 folder in the cluster bucket (/domainConfig) with domain files, named exactly as domains (wildcards allows)
---------------- contains the application name that handles that domain
---------------- cache backed by file download and open
-------- application Db, each application has its own Db within the cluster datasource
-------- controlServer - similar to the legacy server, listens for commands from clients like websites and ccCmd, like runAsyncCmd. Only one per cluster
------------ runs a process timer that queries all app Db for addons that need to run
------------ has a tcp listener to run remote commands like runAsyncAddon or getNextAsyncCmd from quere
-------- asyncCmds - commands run in the background, typically addons set to go off periodically
------------ asyncCmdQueue, a messaging queue to handle asyncCmds  (non-web based, non-blocking commands like housekeeping and process addons)
------------ asyncCmdHandler, a windows service that reads from the asyncCmdQueue and executes them
------------ asyncCmdFeed, a windows service that reads from each application Db and sends commands to the asyncCmdQueue
-------- cacheing
------------ dotnet syste.runtime.caching.memoprychacheclass, (later) memCacheD, a caching api
------------ used to backup the cp cache api, as well as properties
------------ used to hold cluster configuration
---------------- so webRole can quickly read appConfiguration from just domainName
---------------- so cmdProcessHandler and read appConfiguration from just appName
-------- monitor, monitors the apps and environment of the cluster
---- local mode (Server mode, stand alone mode, fallback in case this does not work) 
-------- runs on a single server, single site
-------- a developer has this installed on his machine to build applications
------------ optionally, cdn can point to a different local site
-------- fallback position for small site, budget, or unforseen complication
-------- htmlRenderMethod,  come from an httpModule in an iis site
------------ only delivers dynamic content (html), never static resourses (css,img,js,etc)
------------ creates, configures and initializes cp, then calls an addon based on the url domain (each domain gets one)
------------ primary html interface
------------ gets access to Db, cache and S3 resources with permissions in configuration file
------------ only writes files using api (logs, publicFile, privateFile), never directly to instance
------------ src tags in the html point to cdn (should be handled in deployment process)
-------- remoteMethod,  come from an httpModule in an iis site
------------ only delivers dynamic content (json,text,html), never static resourses (css,img,js,etc)
------------ creates, configures and initializes cp, then calls the remote method based on url path
------------ gets access to Db, cache and S3 resources with permissions in configuration file
------------ only writes files using api (logs, publicFile, privateFile), never directly to instance
------------ src tags in the html are altered during rendering
-------- cdn is a the same site as the htmlRenderMethod (inetpubs/appName/wwwRoot)
-------- private static content is a folder (inetpubs/appName/privateFiles) on the server
------------ website access is granted by copying the files to a folder on the wwwRoot that will be deleted after a time period
------------ if text buffering files are stored here, they are delivered indirectly from the htmlRenderMethod or remoteMethod
------------ apps addons are stored here
-------- configuration files
------------ clusterConfig.json - installs in \ProgramData\cfw with HtmlRenderMethod and remoteMethods
---------------- points to cluster's appConfig and appDomain Lists
---------------- contains a setting that tells the site to look into \ProgramData\cfw\local file system for 
------------ appConfig files - same as cluster mode
------------ domainConfig files - same as cluster mode
-------- application Db, each application has its own Db within the local datasource
-------- controlServer - similar to the legacy server, listens for commands from clients like websites and ccCmd, like runAsyncCmd. Only one per cluster (server)
------------ runs a process timer that queries all app Db for addons that need to run
------------ has a tcp listener to run remote commands like runAsyncAddon or getNextAsyncCmd from quere
-------- asyncCmds - commands run in the background, typically addons set to go off periodically
------------ asyncCmdQueue, (same as today), within the server there is an interal queue. 
------------ asyncCmdFeed, (same as today), server has a process timer, queries app databases and adds commands to queue
------------ asyncCmdHandler, (same as today), when a command is added, it executes ccCmd which reads all commands from server until empty.
-------- cacheing
------------ dotnet syste.runtime.caching.memoprychacheclass, (later) memCacheD, a caching api
------------ used to backup the cp cache api, as well as properties
------------ used to hold cluster configuration
---------------- so webRole can quickly read appConfiguration from just domainName
---------------- so cmdProcessHandler and read appConfiguration from just appName
-------- monitor, monitors the apps and environment of the cluster (server)

2) Clients
---- webRole
-------- website model -- domain points to aspnet webRole. A hit creates cp which renders the request
-------- webapp model -- domain points to static publicStore (s3). Diffrent app.domain points to aspnet app for remoteMethods (but has /admin for data, tools, etc.)
---- scheduler
-------- always running application (service) that runs addons when scheduled (cached addon list checked every second?)
-------- when a task needs to run, it is added to the taskQueue
---- taskQueue
-------- a message query fed by scheduler and both webRoles and workerRoles, consumed by one or more worerRoles
---- workerRoles
-------- reads from taskQueue and kicks off cmd that runs addons

3) Software Architecture
-- client (iis, php, node) - client that needs document (it only creates cp)
---- cp (The published api)
-------- constructor requires an authenticationToken - will become an api authentication -- authenticate developer and grant access
---------- rootAccess - can do anyting, appAccess - only has access to your application
-------- (cp is an api, wrapping cpCoreClass and giving access to cpCore and .core.appservices and .core.clusterservices
-------- (on construction, it creates .core from cpCoreClass which does all the work)
-------- (this object is passed around internally giving access to the namespace)
-------- (cp.core is public, but addons use cpBase so they have no access)
------ cpCore - The primary, top level namespace in the architecture
-------- (cpCore is passed around, care must be taken so cpCore.executeAddon() has access to cp)
-------- clusterServicesClass - provides resources to control the system
---------- two methods return pointers, public cp.cluster which is limited by your access rights, and friend cpCore.clusterAppUser for internal use
-------- appServicesClass - provides resources specific to the application (file, db)
----------- database access, file access, cache
----------- use appservices constructor and use it
-------- all helper classes are created on-the-fly and not persisted like appservices and cluster
---------- cpCore.functions
---------- cpCore.appServices.functions
---------- cp.clusterServices("")Services.functions

3.5) Software Process Flow
---- clients create an object from cpClass and always use that interface for all calls.
-------- aspnet client written by developer for webRole
------------ simple website run on pageManager addon is a simple 10 line boilerplate site, but applicaiton settings must be create
------------ aspnet webrole client can be elaborate aspnet routines that use cp like a library
------------ aspnet webrole client can be simple calls into addons
-------- taskRunner
------------ executes addson from taskQueue
-------- testRunner

-- Always start with a new cp
---- cp = new cpClass; if ( !cp.clusterOk ) { /*clusterfail*/ } else { /*clustersuccess*/ cp.context populate; cp.init(); if ( !cp.appOk ) { /* app fail */ } else { return cp.execute()  }}
---- ( for later security ) 
------ cp creator requires a 'credentialKey' that authenticates a cluster user. That user has rights based on the key to apps in the appList
------ on the local server, store the key for the server in a folder that only the server can reach (with windows permissions)
------ only cp class is public, and it requires a key. All other classes can only be created within this project (when cpCore lives in cp)
---- RENAME CP.INIT() TO CP.APPINIT() -- create cp gets you to cluster, appInit() connects to an app. You can iterate through all of them with cluster.appList collection
---- cp.new creates cp.core, which creates cp.clusterServices("") reading cluster config
------ (if cluster fails, cpCore.cluster.ok=false, need a createCluster process or standalone)
---- cp.context - populate
---- cp.init() 
-------- calls cpCore.init(), which initializes cpCore.app
-------- app configuration is read from cache by either domain name or application name
------------ prewritten cache, backed by a lazy cache/
---------------- appConfig.json is in appFolder, in \ProgramData\cfw\apps\{appName}\appConfig.json
---------------- domainAppIndex file is a list of files with domain as key and appName as content, in /circumference/cluster###/domainAppIndex/
-------- appConfiguration written into appServicesClass (kernalclass, kernelModule will be rolled into appServices)
------------ during appServices constructor, attempt to deserialize from cache (config, etc)
---------------- each serialized object contains
------------ if appServices cache miss, load app config from ContensiveServer (config.xml data), then the rest of appServices from Db
------------ application configuration values (those you may want to change, like odbc connection string) are provided by ContensiveServer, read from config files( later json, later one app per file)
-------- appServices loads by 
------------ opening config file, these values cannot be internally calculated, include endpoints, etc
------------ deserializing internally during construction. Later move to ondemand deserializing
------------- Lazy cache load, so all code necessary to build cdefs, etc are within appservices.
---- cp.executeRoute()
-------- 1) if a query method presented, it is executed (method=login)
-------- 2) if an internal route presented, it is executed (/admin)
-------- 3) if a remote method, execute
-------- 4) if a Link Forwad route, redirect
-------- 5) execute default addon, or pageManager

3.6) IIS Client Pattern (generally a response to a UI request, focused on delivering a document response to a request)
-- create new cp
-- load cp.context object
-- cp.initApp()
-- cp.executeRoute()
-- unload response into script framework
-- cp.dispose()

3.7) Stand-alone client pattern (no UI involved)
-- create new cp
-- load cp.context object
-- cp.initApp()
-- any cp method required, including cp.utils.executeAddon()
-- cp.dispose()

4) Contensive5 Applications
---- collections of settings and resources
---- config values from (S3) and cached
------- config values are setup values, not set within user environment but by developer
------- webrole look up config by domain name
------- ccCmd looks up config by appName to initialize

5) File stores (needs to be reviewed, see files section at top - \ folder layer removed)
---- "path" includes the closing slash, you save a file to a path by just adding the filename to the end
---- "folder" ends without a slash.
---- common to both modes
-------- programDataFolder = determined by system, typically c:\ProgramData\cfw
------------ cpCore.cluster.programDataFiles writes to {programDataFolder}
x-------- programBinFolder = {programDataFolder}\bin - all dll required to run instance (tools in programfiles)
x-------- logFolder = {programDataFolder}\logs
-------- clusterConfigFile = {programDataFolder}Config.json (content is serialized clusterConfigClass)
-------- resourcesFolder = {programDataFolder}\resources
------------ re-installed with each code version update
------------ {resources}\baseCollection.xml - base collection for a new site, installed with each code version updae
------------ {resources}\common\public - files that deploy to /public for all patterns during install (not upgrade)
------------ {resources}\common\public\cclib - copied into \pubiic\ccLib during both install and upgrades
------------ {resources}\common\private - files that deploy to /public for all patterns during install (not upgrade)
-------- appPatterns = {resources}\appPatterns\
------------ created during install (from github \installSource folder)
------------ {appPatterns}\patternName\public - named for each pattern, holds files like .php copied to /public during install (not upgrade)
------------ {appPatterns}\patternName\private - named for each pattern, holds files copied to /private during install (not upgrade)
x-------- localDataPath = in clusterConfigFile, typically d:\cfw\
x-------- localLogPath = {localDataPath}logs\
x-------- localEmailOutPath = {localDataPath}logs\
---- scale mode (the cluster of apps is hosted across multiple machines)
-------- clusterFolder = s3/{clusterName}, clusterName stored in clusterConfigFile
------------ cpCore.cluster.files reads local cache {clusterFolderLocal}, fails to {clusterFolder}
------------ cpCore.cluster.files writes to {clusterFolderLocal} then copied to {clusterFolder}
------------ clusterFolderLocal = {localDataPath} (where remote files are downloaded locally to be read)
-------- clusterAppsFolder = {clusterFolder}\apps, root folder for all app files (added to prevent app name collision with other resources)
-------- appDomainFolder = {clusterFolder}\appDomainFiles
-------- appFolder = {clusterAppsFolder}\{appName}
------------ this is where the application is stored. for node, this is the root folder
------------ appConfig.json is stored here
-------- appPublicFolder = {appFolder}\public, stores all static content
------------ cpCore.app.cdnFiles writes to {appPublicFolder}
-------- appPrivateFolder = {appFolder}\private, stores non-shared app files (private, content, addons)
------------ cpCore.app.privateFiles writes to {appPrivateFolder}
-------- appAddonsFolder  {appPrivateFolder}\Addons, the root for all addons
---- local mode (the cluster of apps is only hosted on one machine)
-------- clusterFolder = clusterFolderLocal = {localDataPath}cluster\
------------ cpCore.cluster.files reads/writes {clusterFolderLocal}
-------- clusterAppsFolder = {clusterFolder}\apps, root folder for all app files (added to prevent app name collision with other resources)
-------- appDomainFolder = {clusterFolder}\appDomainfiles
------------ appConfig.json is stored here
-------- appFolder = {clusterAppsFolder}\{appName}
------------ this is where the application is stored. for node, this is the root folder
-------- appPublicFolder = {appFolder}\public, stores all static content
------------ cpCore.app.cdnFiles writes to {appPublicFolder}
-------- appPrivateFolder = {appFolder}\private, stores non-shared app files (private, content, addons)
------------ cpCore.app.privateFiles writes to {appPrivateFolder}
-------- appAddonsFolder  {appPrivateFolder}\Addons, the root for all addons


6) n/a

7) Hosts are programs that call Contensive to get results
---- initially the contesive hosts are IIS (module) and ccCMD (standalone). 
---- Add nodeJs with EdgeJ, or iisNode to run node within iis

8) Development pattern / best practice
---- give preference to 
-------- dotnet name spaces over 3rd party
-------- hosted services over 3rd party installs
---- Internally, cp creates an environment object (appservices) which is used by all internal classes.
-------- pick either passing it in during construction, or as an argument

10) Addons
---- Installs to appPrivateFiles, unzipping the entire contents there for reference
---- wwwFiles and contentFiles are copied to s3/public (site static content)
---- then the assemblies are copied to the webroles
- 
11) description of the web role process
---- the web role starts with a hit to the iis Module, with just the domain name
---- the iis module has some kind of a special privelage that lets it request the appConfig with just the domain name
---- the appconfig returns with key(s) (passwors) that let it communicate with that app's Db, files, cache, etc.
---- the iis module creates cp
-------- call getAppConfig, which reads the local app config file and returns its object
-------- config file on local machine (cache with expiration) checked for config file (json file) for this appname
------------ if fail, use s3 sdk to download config file (not available in instance mode)
-------- with appConfig, it gets resource locations like appCache and file store (contentfiles)
-------- reads environment (cdefs etc that were in kernel and appServers in version 4)
---- host populates cp.context
---- host calls cp.init() to initialize state (visit, visitor, user)
---- host calls and cp method needed

12) describe server role process
---- server runs a process timer, periodically checking for next addon for each application
-------- when one needs to be run, it queues it up to execute and spawns a process to execute it.
-------- if in instance mode, this kicks off a cmd process that reads the queue and executes the addon
-------- if in scale mode, there are cmd VMs that read from the queue.

12) creating a new webrole instance
---- consider using ec2 bootstraping to start a golden 'baked' AMI
---- use IAM roles to grant permissions to the instance
---- then EC2Config service to run a powershell that copies down addon assemblies

13) webRole configuration
---- iis with iisModule installed
---- no physicalWWWroot, there is none. IIS module creates cp which calls defaultAddon, which gets html from [Db, cache, build cache from template on s3]. URLWrite static Domain.
---- no contentFiles, there is none. static content [from design or contentsystem] comes from s3.
---- no db path, Db must be remote so it cannot be access, only sql server.
---- program files/contensive, executables
---- program data/logs, ??
---- program data/emailout
---- no /cclib mapped to iis, this is static content mapped from within the s3 static content folder

14) What is in the AppConfig data
---- the app name
---- the static domain for url rewrite
-------- scale mode, the domain to the s3 data
-------- instance mode, empty (turns off rewrite)
---- the default connection string [whatever is needed to make the default Db connection]
---- the path to write to the static content [endpoint, path, whatever is needed]
-------- instance mode, just the physical path to the wwwRoot
-------- scale mode, key to a method that lets the app on the webrole read from the s3 static storage.

15) file storage
---- wwwRoot becomes the s3/public bucket.
---- private files are in the s3 bucket in a /private folder
---- Content Files are treated two ways, /public and /private
-------- during site migration, all contentFiles will be considered /public
-------- anything that is public goes to the static content on s3, /public folder
------------ design files
------------ files added for wwwRoot part of addons
------------ files uploaded by a content record, like images
-------- anything for the site to use, but is not within the wwwroot goes on s3, /private folder
------------ when developers setup /private storate, it is up to them to cache
----- wwwRoot files go 
--------

16) fields types that store large text on old contentfile store now saves it to s3 storage

17) Application Db
---- initially Db will be Sql Server backed EDS
-------- to get up and runnign fast because code written for sql server, and RDS interface in AWS SDK
-------- eventually might go to mysql for cost, or ec2 based servers
---- each application gets its own Db
---- the app configuration contains a connectionstring (endpoint, etc) to the Db

18) maintain two modes, scale mode and instance mode
---- scale mode
---- instance mode
-------- like now, static content in wwwroot
-------- dynamic content from iis module

19) trace logs
---- ?? they are generated by all the processes (webrole, 

20) How addons are executed

21) Integrate the html parser on nuget (google it)

22) consider a static head
---- when you install a collection, it adds styles and js to the head for each template. 
---- the entire head is built for each template, allowing for the addition of a page-related element, to include title, meta.
---- a rebuild process composes the template as ready for use as possible
-------- triggered on a filewatch in an ftp upload forlder
-------- triggered by an addon save, css save, js save, etc.
---- remove methods that add to head, add to the /body.

23) Development environment 
---- easy to collaborate
-------- design should havea git-design repo they store everything without copy/paste
-------- server-dev same thing
-------- maybe front-end dev same thing
-------- if front-end needs new  server code, they should be able to close the dev-repo and all done. 




? Where are wwwroot files stored, because you cannot mount a common file system to EC2
---- files from designer files 
---------- could ftp to s3
-------------- rewrite static urls in the html to point to s3 as a static content server
---- files from addon installation. - 
---------- uploads go directly to s3, unzipped like it is now, but with the application in the /private folder
-------------- static content is copied to /public
-------------- contentFile files need to be set as either /publc (mapped into wwwroot) or /private (in s3, site can read, not http accessable)
-------------- assemblies stay /private (how addons are run #
---------x could a filewatcher upload to s3, then distribute out to the webroles
---------- assemblies upload to s3 and are copied to webrole instance during install
---------- when an addon is called, if the collection is not in the /addons folder, it is downloaded dynamically from s3
---- put them on the webrole or not
-----x A on the webrole instance
--------x will have to either make it into the AMI, or copied to a persistant store to repopulate future webrole instances
--------x will have to quickly be copied to other webrole instances
------ B not on the webrole
-------- stack overflow serves all static content from another cookieless server. This comes from yahoo, https://sstatic.net/
----------- yahoo - Create a subdomain and host all your static components there.
----------- Yahoo! uses yimg.com, YouTube uses ytimg.com, Amazon uses images-amazon.com and so on.

---- possible - rewrite html doc src to cdn.domain.com and put the domain on s3. ftp, etc goes to s3, filewatcher copys to webrole instances
---- possible - use s3fs or sdk to copy s3 root folder to webrole instances

??? Where are content files stored
---- you cannot mount a common file system
----- files come from file uploads in resource library
----- file uploads within content
----- files created from contentfields textfilename, javascript and css - possible put these in wwwroot
----- files saved within the application using cp.file.save[virtual]

?? Where is the config.xml file stored
---- make is json not xml
---- each app has it's own config, all stored in a bucket on s3
---- during cp init(), the cp client requests the appconfig by its name. No other process can request it so no other process can get to app's db, cache, files, etc.

Dim dt As DataTable
dt = asv.executeSql(SQL)
If dt.Rows.Count = 0 Then
Else
    value = EncodeInteger(dt.Rows(0).Item(0))
    For Each dr As DataRow In dt.Rows
        value = EncodeInteger(dr("value"))
        For Each dc As DataColumn In dt
            value = EncodeInteger(dr(dc))
        Next
    Next
End If


Pattern for error handling

try
	...
catch (ex)
    cpCore.handleException( ex)
    If Not cpCore.trapErrors Then
        Throw New ApplicationException("rethrow", ex)
    End If
end try
