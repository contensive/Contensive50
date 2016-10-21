
var http = require('http'),
    url = require('url'),
    path = require('path'),
    fs = require('fs'),
    edge = require('edge'),
    static = require('node-static'),
    fileServer = new(static.Server)(),
    clusterConfigJSON = fs.readFileSync('.\\clusterConfig.json'),
    clusterConfig = JSON.parse(clusterConfigJSON),
    appConfigs = [],
    appsPath,
    clusterFolders,
    srcFilename,
    requestCnt = 0,
    domainXRef = {};
// load from file -- http://www.sitepoint.com/web-foundations/mime-types-complete-list/
var mimeTypes = {
    "html": "text/html",
    "css": "text/css",
    "js": "text/javascript",
    "jpeg": "image/jpeg",
    "jpg": "image/jpeg",
    "png": "image/png",
    "gif": "image/gif",
    "endoflist": "text/html"
};
//
// local cluster config
//
clusterConfigJSON = fs.readFileSync('.\\clusterConfig.json');
console.log( 'clusterConfigJSON='+clusterConfigJSON);
clusterConfig = JSON.parse(clusterConfigJSON);
//
// iterate through apps subfolders 
//  build appConfigs by reading each appConfig.json
//  as it builds, build domainAppXref object
//      { "domain":"appName", etc. )
//  then during request appname=domainAppXref[appname]
//     or similar
appsPath = path.join( process.cwd(), 'apps' );
clusterFolders = getDirectoriesSync( appsPath );
//console.dir( clusterFolders );
for (var i = 0, len = clusterFolders.length; i < len; i++) {
    var appName, domainName;
    srcFilename = path.join( appsPath, clusterFolders[i]);
    srcFilename = path.join(srcFilename,'appConfig.json');
    //console.log( 'srcFilename='+srcFilename );
    try {
        data = fs.readFileSync(srcFilename);
        try {
            var o = JSON.parse( data );
            try {
                appName = o.name.toLowerCase();
                for ( var i=0, len = o.domainList.length; i < len; i++ ) {
                    domainXRef[ o.domainList[ i ].toLowerCase() ] = appName;
                }
            } catch( ex ) {
            }
            appConfigs.push( o );
            //console.log( 'appConfig='+data );
        } catch(ex) {
            console.log( 'error parsing appConfig for '+srcFilename + ', '+ex.message );
        }
    } catch(ex) {
        console.log( 'appConfig read error' + ', '+ex.message );
    }
};
console.log( 'waiting...' );
//console.dir( domainXRef );
//
// create listener
//
http.createServer(function(request, response) {
    var requestPtr = requestCnt++,
    requestContext = {},
    mimeType,
    fileExt;
    //
    console.log( '----- new request #'+requestPtr );
    //
    var hostName = request.headers.host;
    console.log( '[' + requestPtr + ']hostName='+ hostName);
    //
    var appName = domainXRef[ hostName.toLowerCase() ];
    console.log( '[' + requestPtr + ']appName='+appName );
    //
    var appRootPath = path.join( appsPath, appName + '\\public' )
    var legacyVirtualPath = '/' + appName + '/files';
    var requestPathPage = url.parse(request.url).pathname;
    if ( requestPathPage.toLowerCase().indexOf( legacyVirtualPath )==0 ) {
        requestPathPage = requestPathPage.substr( legacyVirtualPath.length );
        console.log( '[' + requestPtr + ']ERROR, legacy virtual path [' + legacyVirtualPath + '] found/removed');
    }
    if ( requestPathPage.toLowerCase().indexOf( '/files/' )==0 ) {
        requestPathPage = requestPathPage.substr( 6 );
        console.log( '[' + requestPtr + ']ERROR, incorrect legacy virtual path [/files] found/removed');
    }
    console.log( '[' + requestPtr + ']requestPathPage='+ requestPathPage);
    console.log( '[' + requestPtr + ']requestPathPage='+ requestPathPage);
    //
    var filename = path.join(appRootPath, requestPathPage);
    console.log( '[' + requestPtr + ']filename='+ filename);
    //
    var testFilename = filename;
    if ( testFilename.substr(testFilename.length - 1 )  =='\\' ) {
        testFilename = path.join( testFilename, 'index.html' );
    }
    fs.exists(testFilename, function(exists) {
        if(exists) {
            //
            // consider static file server here -
            // http://www.bennadel.com/blog/2818-learning-node-js-building-a-static-file-server.htm
            //
            console.log( '[' + requestPtr + ']static content');
            var appRequestPathPage = '/apps/' + appName + '/public' + requestPathPage;
            console.log( '[' + requestPtr + ']appRequestPathPage='+appRequestPathPage );
            fileServer.serveFile( appRequestPathPage, 200, {}, request, response);

//fileExt = path.extname(filename).split(".")[1].toLowerCase();
//mimeType = mimeTypes[fileExt];
//response.writeHead(200, {'Content-Type':mimeType});
//var fileStream = fs.createReadStream(filename);
//fileStream.pipe(response);
//return;

            //fileExt = path.extname(filename).split(".")[1].toLowerCase();
            //mimeType = mimeTypes[fileExt];
            //console.log( '[' + requestPtr + ']mimeType='+mimeType );
            //response.writeHead(200, {'Content-Type':mimeType});
            //console.log( '[' + requestPtr + ']filename='+ filename);
            //var fileStream = fs.createReadStream(filename);
            //fileStream.pipe(response);
            //response.end();
            //return;
//        } else if( requestPathPage.toLowerCase().indexOf( 'favicon.ico' )!=-1 ) {
//            //
//            // 404 the favicon.ico 
//            //
//            console.log( '[' + requestPtr + ']favicon.ico');
//            response.writeHead(404, {'Content-Type': 'text/plain'});
//            response.write('404 Not Found\n');
//            response.end("");
        } else {
            //
            // cfw dynamic content
            //
            console.log( '[' + requestPtr + ']dynamic content');
            //console.log( '[' + requestPtr + ']domainXRef[ hostName ]='+ domainXRef[ hostName ]);
            requestContext.appName = appName;
            requestContext.domain = request.headers.host;
            requestContext.pathPage = requestPathPage;
            //
            getHtmlDocProxy( requestContext, function( error, docHtml ) {
                //console.log('getHtmlDocProxy done');
                if (error) {
                    console.log('[' + requestPtr + ']ERROR');
                    response.writeHead(200, {'Content-Type': 'text/plain'});
                    response.write('404 Not Found\n');
                } else {
                    console.log('[' + requestPtr + ']SUCCESS');
                    response.writeHead(200, {"Content-Type": "text/html"});
                    response.write( docHtml );
                }
                //console.log("done, docHtml=" + docHtml );
            })
            response.end("");
            console.log( '[' + requestPtr + ']dynamic content, response.end');
        }
    }); //end path.exists
}).listen(80);
//
//
//
var getHtmlDocProxy = edge.func({
    references: [
        'c:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Data.dll'
        ,'..\\bin\\cfw.dll' 
        ,'..\\bin\\cpbase.dll' 
    ]
    ,source: function() {/*
        using System.Threading.Tasks;
        //public class requestContextClass {
        //    public string appName = "";
        //    public string domain = "";
        //    public string pathPage = "";
        //}
        public class Startup {
            public async Task<object> Invoke(dynamic requestContext) {
                string returnDocHtml = "";
                Contensive.Core.CPClass cp = new Contensive.Core.CPClass();
                if (!cp.clusterOk) {
                    // cluster fail
                } else {
                    //
                    // populate cp.context
                    //
                    cp.Context.allowProfileLog = false;
                    cp.Context.isBinaryRequest = false;
                    cp.Context.requestNameSpaceAsUnderscore = false;
                    cp.Context.requestNameDotAsUnderscore = false;
                    cp.Context.domain = (string)requestContext.domain;
                    cp.Context.pathPage = (string)requestContext.pathPage;
                    //
                    // initialize application
                    //
                    cp.init( (string)requestContext.appName );
                    if (!cp.appOk) {
                        // app not running
                    } else { 
                        // appSuccess, execute default addon
                        // 
                        returnDocHtml = cp.execute();
                    }
                }
                //cp.Dispose;
                return returnDocHtml;
            }
        }
    */}
})

function getDirectoriesSync(srcpath) {
  return fs.readdirSync(srcpath).filter(function(file) {
    return fs.statSync(path.join(srcpath, file)).isDirectory();
  });
}
